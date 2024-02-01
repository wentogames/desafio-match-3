# Match-3

![Match-3](/Match3.png?raw=true "Match-3")

Developed in Unity 2020.3.33f1

## Links
- https://unity.com/releases/editor/archive

- ------------
Vanessa Version

- I have initially made changes to the UI, adding punctuation, phase number and buttons to the boosts asked in the challenge;
- The original codes were not removed, but logic was added to consider if any boost was being used at the touch;
- Following the "a touch can not be done while animation is happening", a boost cannot be activated to be used if an animation is going on.
* On GameController.cs, the "SwapTile" method should be broken into smaller methods (ideally one for each BoostMode). This is noted as "TODO".
- An audio clip was added. It is played every time a match is done and a piece is removed, giving the user a feedback that a valid move was done. The more combos are consecutively done, the higher the audio clip pitch gets, emphasizing the combo accumulation achievement sense.
- My next step would be to add progressive phases. The player would need an specific amount of points to go to next phase (otherwise would remain in the same). After some phases, the number of different tiles would increase (currently is blue, green, yellow and orange. On the project you could already find red, pink and purple tiles, it is only a matter of implementing other colors on the scene start). One of the possibles solutions is to load the scene again, resetting the board (and keeping the phase index in PlayerPrefs or in any other way that would be kept on "Don't destroy on load").
