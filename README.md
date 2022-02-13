# DialogueModule

[Readme em português](README_PORTUGUÊS.md)

## How to install
- Using the Package Manager interface

![image](https://user-images.githubusercontent.com/10902660/153759347-7959671b-517c-4c6f-8cf0-1b6ed2c5b7e5.png)

Use the url ```https://github.com/FellowshipOfTheGame/DialogueModule.git``` and press add

![image](https://user-images.githubusercontent.com/10902660/153759448-f436817a-42ce-49a5-bbfd-fca1406b8ede.png)

- Editing the Packages/manifest.json file directly

Make sure this project is in the dependencies list as shown below and open the project as usual:

    {
      "dependencies": {
        "com.fellowshipofthegame.dialoguemodule": "https://github.com/FellowshipOfTheGame/DialogueModule.git"
      }
    }

## How to use

The Example sample included in the package has a scene and dialogues with 3 use cases possible, as well as 2 prefabs that can be copied and changed as needed.

The example is based on the [FinalInferno](https://github.com/FellowshipOfTheGame/FinalInferno) use of the package, where we inherit the base Dialogue and OptionsDialogue classes to add extra behaviour when starting/ending dialogue instead of pausing the game.

If the desired effect is the one from [Anathema](https://github.com/FellowshipOfTheGame/anathema) the base classes would suffice, all you need to do is check the pauseDuringDialogue option in the DialogueHandler.
