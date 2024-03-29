﻿using BepInEx;
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
		public const string version = "0.1.6";

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
				Vector3 original, Vector3 target, float delta, CharacterMotor character)
		{
			if ( character.acceleration <= 0 ) return original;
			else if ( ! character.isGrounded || character.isAirControlForced )
			{
				Vector3 velocity = original;
				if ( ! character.isFlying )
					target.y = velocity.y = 0;

				CharacterBody body = character.body;
				float speed = Mathf.Max(target.magnitude, body.moveSpeed),
						current = velocity.magnitude, increase = current / speed;

				if ( ! body.isSprinting )
					speed *= body.sprintingSpeedMultiplier;

				GenericSkill hook = body.skillLocator?.FindSkillByFamilyName(
						"LoaderBodySecondaryFamily");

				if ( character.disableAirControlUntilCollision && ! character.isFlying )
				{
					if ( ! body.isPlayerControlled || target == Vector3.zero )
						return original;

					if ( hook && hook.stateMachine )
					{
						var state = hook.stateMachine.state as EntityStates.Loader.FireHook;
						if ( state?.isStuck is true )
							return original;

						delta *= 0.75f / Mathf.Pow(character.acceleration, 0.25f);
					}
					else
					{
						delta /= 1.5f;
						if ( original.y > 0 )
							delta /= Mathf.Pow(1.25f, original.y * original.magnitude / 750);
					}
				}
				else increase = ( 1 + increase ) / 2;

				if ( speed != 0 && increase > 1 )
					target *= increase;

				increase = current - speed;
				if ( increase > 0 && hook is null )
				{
					float angle = 0.5f;
					if ( target != Vector3.zero )
						angle *= 1 - Vector3.Dot(velocity.normalized, target.normalized);

					delta *= 1 + increase / ( character.acceleration * 0.15f ) * angle;
				}

				if ( ! character.isFlying )
					target.y = original.y;
			}

			return Vector3.MoveTowards(original, target, delta);
		}

		[HarmonyPatch(typeof(Transport), nameof(Transport.OnEnter))]
		[HarmonyPrefix]
		private static void Unlock(EntityState __instance) =>
				__instance.characterMotor.disableAirControlUntilCollision = false;
	}
}
