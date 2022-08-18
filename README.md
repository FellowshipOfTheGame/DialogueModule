# DialogueModule

[Readme em português](README_PORTUGUÊS.md)

## __How to install__
- __Using the Package Manager interface__

![image](https://user-images.githubusercontent.com/10902660/153759347-7959671b-517c-4c6f-8cf0-1b6ed2c5b7e5.png)

Use the url ```https://github.com/FellowshipOfTheGame/DialogueModule.git#upm``` and press add

![image](https://user-images.githubusercontent.com/10902660/153759448-f436817a-42ce-49a5-bbfd-fca1406b8ede.png)

- __Editing the Packages/manifest.json file directly__

Make sure this project is in the dependencies list as shown below and open the project as usual:

    {
      "dependencies": {
        "com.fellowshipofthegame.dialoguemodule": "https://github.com/FellowshipOfTheGame/DialogueModule.git#upm"
      }
    }

- __Installing a specific version of the package (Recommended)__

If you want to install a specific version of the package follow the instructions above, but replace #upm with the desired version.

The url in the first example would be ```https://github.com/FellowshipOfTheGame/DialogueModule.git#1.0.3``` for version 1.0.3.

## __How to use__

The Example sample included in the package has a scene and dialogues with 3 use cases possible, as well as 2 prefabs that can be copied and changed as needed.

The example is based on the [FinalInferno](https://github.com/FellowshipOfTheGame/FinalInferno) use of the package, where we inherit the base Dialogue and OptionsDialogue classes to add extra behaviour when starting/ending dialogue instead of pausing the game.

If the desired effect is the one from [Anathema](https://github.com/FellowshipOfTheGame/anathema) the base classes would suffice, all you need to do is check the pauseDuringDialogue option in the DialogueHandler.

## __How to contribute__

After changes are pushed to the main branch of the repo, a [github action](.github/workflows/UpdateUPMBranch.yml) will automatically update the upm branch with the proper directory structure for a release. After that it's just a matter of doing a new release pointing to that upm branch.

When creating a new release, it is necessary to update the version number in the [package.json](Assets/UPM/package.json) file using [semanting versioning](https://semver.org/) for the number (_major_._minor_._patch_). The tag for the new release must also correspond with this new version number.
