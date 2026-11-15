# vr-raycast-selection

Ray-casting selection and a button counter panel for Unity and Meta Quest 2.

## Features

- Ray-casting from the controller, with hover highlight and trigger selection
- Two-state push buttons that change color and recess when pressed
- A counter panel showing the number of pressed buttons
- Procedurally generated button layouts on a plane or a sphere

## Requirements

- Unity 2021.3
- Oculus Integration with `OVRCameraRig`
- Meta Quest 2

## Installation

Clone the repository into the `Assets` folder of a Unity project:

```bash
cd <UnityProject>/Assets
git clone https://github.com/Yanis01682/vr-raycast-selection.git
```

## Setup

1. Add `RayPointer` to `OVRCameraRig/TrackingSpace/LeftHandAnchor` or `RightHandAnchor`.
2. Create an empty GameObject and add `ButtonPanel`.

| Layout | Columns × Rows | Position |
|---|---|---|
| Sphere | 30 × 20 | Sphere center at head height |
| Flat | 4 × 3 | In front of the user, e.g. (0, 1.3, 1.5) |

3. Enter Play mode.

Without a connected headset, `RayPointer` falls back to mouse input from the main camera.

## Parameters

`ButtonPanel`

| Parameter | Description | Default |
|---|---|---|
| Radius | Sphere radius (m) | 2.5 |
| Horizontal Span / Vertical Span | Angular extent of the sphere layout (°) | 110 / 60 |
| Fill | Fraction of each cell occupied by a button | 0.94 |
| Button Width / Height / Gap | Button size and spacing for the flat layout (m) | 0.14 / 0.09 / 0.006 |
| Button Depth | Button thickness (m) | 0.04 |
| Press Ratio | Recess distance as a fraction of thickness | 0.6 |
| Released / Pressed Color | Button colors | Orange / Red |

`RayPointer`

| Parameter | Description | Default |
|---|---|---|
| Hand | Controller whose trigger is read | Right (Left when attached to `LeftHandAnchor`) |
| Max Distance | Ray length (m) | 20 |

## Structure

```
Scripts/
├── Pointable.cs     Base class for selectable objects
├── RayPointer.cs    Ray-casting, highlighting and trigger input
├── PushButton.cs    Button state and recess
└── ButtonPanel.cs   Button layout and counter panel
```
