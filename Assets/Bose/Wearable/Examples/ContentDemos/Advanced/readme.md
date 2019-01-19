# Advanced Demo
## Summary
This demo provides a more advanced use-case of using the BoseWearable-Unity plugin by demonstrating a simple gameplay mechanic where you swivel your head in several directions to collect objects. 

## Functionality
A rectangular widget and a cone are positioned at the center of the screen, pointing towards the view direction of the Bose AR device. The radius of the cone represents the orientation uncertainty of the hardware. A wireframe polyhedron surrounds the cone with visible vertices and edges, representing virtual space around the user’s head.

At the beginning of play, we run a simple calibration by asking players to look straight ahead and keep their head still. After the Bose AR device reports minimal movement, the capsule and cone will match orientation with the player's head rotation. 

Periodically, a glowing target appears at one of the vertices, and sound begins playing from the target. The player must orient their Bose AR device to face that direction; upon reaching it (within a threshold), the target will grow in scale. Sustaining that orientation for a number of seconds will cause it to be collected and a new point to be spawned.