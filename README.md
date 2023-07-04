## Introduction

In some circumstances players may encounter a phenomenon often referred to as **"momentum lock"**. The easiest way to experience this is by simply stepping onto a vent/geyser or other launch pad - resulting in complete loss of air control until your feet touch the ground. However, certain abilities also induce this state, and a similar effect can be seen when your current velocity exceeds your movement speed by a significant amount. This is especially pronounced on characters with low acceleration, such as *Artificer* and *MUL-T*.

To compensate, a couple of algorithms have been implemented to counteract these effects. However, care has been taken to retain the original feel of the game in typical situations. Most changes only kick in at high velocity, and scale proportionally. Note that the *Loader* will also receive reduced impact to avoid compromising this character's identity. If, for any reason, you would like to disable (or re-enable) this plugin during a game,
 open the console using **`CTRL`**`+`**`ALT`**`+`**`~`** and enter the command `toggle_momentum`.

## Special Thanks

- **HIFU**, for inspiration. Note that [BetterJumpPads](https://thunderstore.io/package/HIFU/BetterJumpPads) will override some of this behavior.
- **Race**, who has been testing this covertly on [stream](https://www.twitch.tv/race). Appreciate the feedback and patience.
- And **you**! At least, any of his viewers that actually noticed.
