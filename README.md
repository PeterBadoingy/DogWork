# DogWork

DogScript
DogScript is a script for Grand Theft Auto V that adds a companion dog to the game. 
The script allows players to spawn, control, and interact with a loyal Rottweiler companion
Using the Gesture system of Los Santos Red.

Features
Spawn Dog: Use the Scroll Lock key to spawn or remove the dog companion.
Vehicle Interaction: The dog can enter and exit vehicles based on player actions.
Behavior Commands: Command the dog to follow, sit, attack, or lay down using in-game gestures.
Automatic Aggression: The dog will automatically attack aggressive NPCs attacking the player.

Installation:
Ensure you have the latest version of Script Hook Dot Net installed.
Place the DogScript.dll file into your Grand Theft Auto V scripts directory.

Usage + Controls:
Press the Scroll key to spawn or remove the dog companion.
Interact with the dog using in-game gestures for various behaviors.

Notes:
The script uses Los Santos Red in-game gestures to command the dog. 
Add the following to the Gestures.xml locating in Plugins/LosSantosRED

  <GestureData>
    <Name>Follow</Name>
    <AnimationName>gesture_come_here_hard</AnimationName>
    <AnimationDictionary>gestures@m@standing@casual</AnimationDictionary>
    <AnimationEnter />
    <AnimationExit />
    <IsInsulting>false</IsInsulting>
    <IsOnActionWheel>true</IsOnActionWheel>
    <SetRepeat>false</SetRepeat>
    <IsWholeBody>false</IsWholeBody>
    <Category>Dog Control</Category>
  </GestureData>      
  <GestureData>
    <Name>Attack</Name>
    <AnimationName>gesture_bring_it_on</AnimationName>
    <AnimationDictionary>gestures@m@standing@casual</AnimationDictionary>
    <AnimationEnter />
    <AnimationExit />
    <IsInsulting>true</IsInsulting>
    <IsOnActionWheel>true</IsOnActionWheel>
    <SetRepeat>false</SetRepeat>
    <IsWholeBody>false</IsWholeBody>
    <Category>Dog Control</Category>
  </GestureData>      
  <GestureData>
    <Name>Sit </Name>
    <AnimationName>gesture_hand_down</AnimationName>
    <AnimationDictionary>gestures@m@standing@casual</AnimationDictionary>
    <AnimationEnter />
    <AnimationExit />
    <IsInsulting>false</IsInsulting>
    <IsOnActionWheel>true</IsOnActionWheel>
    <SetRepeat>false</SetRepeat>
    <IsWholeBody>false</IsWholeBody>
    <Category>Dog Control</Category>
  </GestureData>
  <GestureData>
    <Name>Lay Down</Name>
    <AnimationName>gesture_bye_soft</AnimationName>
    <AnimationDictionary>gestures@m@standing@casual</AnimationDictionary>
    <AnimationEnter />
    <AnimationExit />
    <IsInsulting>false</IsInsulting>
    <IsOnActionWheel>true</IsOnActionWheel>
    <SetRepeat>false</SetRepeat>
    <IsWholeBody>false</IsWholeBody>
    <Category>Dog Control</Category>
  </GestureData>

      
Credits
DogScript was created by Peter Badoingy and Chat GPT.
