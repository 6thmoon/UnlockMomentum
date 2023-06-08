using BepInEx;
using EntityStates;
using HarmonyLib;
using RoR2;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Permissions;
using UnityEngine;
using Transport = EntityStates.Toolbot.ToolbotDash;

[assembly: AssemblyVersion(Local.Unlock.Momentum.Plugin.version)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]

namespace Local.Unlock.Momentum
{
	[BepInPlugin(identifier, "UnlockMomentum", version)]
	public class Plugin : BaseUnityPlugin
	{
		public const string identifier = "local.unlock.momentum";
		public const string version = "0.1.1";

		public void Awake() => Patch();
		private static Harmony instance = null;

		[ConCommand(commandName = "toggle_momentum", helpText = "Select air control behavior.")]
		private static void Patch(ConCommandArgs args = default)
		{
			if ( args.userArgs is object && args.Count != 0 )
				throw new ConCommandException("No value required.");

			if ( instance is null )
			{
				instance = Harmony.CreateAndPatchAll(typeof(Plugin));
				Debug.Log("Unlocked air control.");
			}
			else
			{
				instance.UnpatchSelf();
				instance = null;
				Debug.Log("Restored default behavior.");
			}
		}

		[HarmonyPatch(typeof(CharacterMotor), nameof(CharacterMotor.PreMove))]
		[HarmonyTranspiler]
		private static IEnumerable<CodeInstruction>
				Transpile(IEnumerable<CodeInstruction> codeInstructions)
		{
			foreach ( CodeInstruction instruction in codeInstructions )
			{
				if ( instruction.Calls(
						typeof(Vector3).GetMethod(nameof(Vector3.MoveTowards))))
				{
					yield return new CodeInstruction(OpCodes.Ldarg_0);
					yield return CodeInstruction.Call(
							typeof(Plugin), nameof(Plugin.Accelerate));
				}
				else if ( instruction.LoadsField(
						typeof(CharacterMotor).GetField(
						nameof(CharacterMotor.disableAirControlUntilCollision))))
				{
					yield return new CodeInstruction(OpCodes.Pop);
					yield return new CodeInstruction(OpCodes.Ldc_I4_0);
				}
				else yield return instruction;
			}
		}

		private static Vector3 Accelerate(
				Vector3 velocity, Vector3 target, float delta, CharacterMotor character)
		{
			if ( character && character.body && (
					character.isAirControlForced || ! character.isGrounded ))
			{
				Vector3 horizontal = velocity;
				horizontal.y = 0;

				float increase = horizontal.magnitude / character.walkSpeed;

				if ( character.disableAirControlUntilCollision )
				{
					if ( ! character.body.isPlayerControlled
							|| character.moveDirection == Vector3.zero
							|| character.name.StartsWith("LoaderBody")
						) return velocity;

					delta *= 0.5f;
				}
				else increase -= delta / character.walkSpeed;

				if ( increase > 1 && character.walkSpeed != 0 )
				{
					target *= increase;
					delta *= 1 + ( horizontal.magnitude - character.walkSpeed ) * (
							1 - Vector3.Dot(horizontal.normalized, target.normalized)
						) / ( character.acceleration * 0.25f );

					if ( ! character.isFlying )
						target.y = velocity.y;
				}
			}

			return Vector3.MoveTowards(velocity, target, delta);
		}

		[HarmonyPatch(typeof(Transport), nameof(Transport.OnEnter))]
		[HarmonyPrefix]
		private static void Unlock(EntityState __instance) =>
				__instance.characterMotor.disableAirControlUntilCollision = false;
	}
}
