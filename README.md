## Introduction

In some circumstances players may encounter a phenomenon often referred to as **"momentum lock"**. The easiest way to experience this is by simply stepping onto a vent/geyser or other launch pad - resulting in complete loss of air control until your feet touch the ground. However, certain abilities also induce this state, and a similar effect can be seen when your current velocity exceeds your movement speed by a significant amount.

To compensate, a couple of algorithms have been implemented to counteract these effects. In order to retain the original feel of the game, most changes only kick in at high velocity, and scale proportionally. As an exception, some features are reduced or omitted on the **Loader** to avoid affecting gameplay significantly. If you would like to disable/enable this plugin during a game, enter the console command `toggle_momentum`.

## Special Thanks

- **HIFU**, for inspiration. Note that [BetterJumpPads](https://thunderstore.io/package/HIFU/BetterJumpPads) will override some of this behavior.
- [**Race**](https://www.youtube.com/channel/UC7sGmlJ87yXmFA9v82x51ZA), who provided feedback and testing prior to public release.

## Version History

#### `0.1.6`
- Use sprint speed instead of walk speed to determine threshold.
- Improve compatibility in certain cases.

#### `0.1.5` **- Initial Release**
