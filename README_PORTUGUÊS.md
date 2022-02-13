# DialogueModule

## Como instalar
- Usando a interface do Package Manager

![image](https://user-images.githubusercontent.com/10902660/153759347-7959671b-517c-4c6f-8cf0-1b6ed2c5b7e5.png)

Insira a url ```https://github.com/FellowshipOfTheGame/DialogueModule.git``` e aperte em add

![image](https://user-images.githubusercontent.com/10902660/153759448-f436817a-42ce-49a5-bbfd-fca1406b8ede.png)

- Editando o arquivo Packages/manifest.json diretamente

Se certifique que este projeto está na lista de dependências como mostrado abaixo e abra o projeto normalmente:

    {
      "dependencies": {
        "com.fellowshipofthegame.dialoguemodule": "https://github.com/FellowshipOfTheGame/DialogueModule.git"
      }
    }

## Como usar

A sample 'Example' incluída no pacote tem uma cena e diálogos com 3 casos de uso possíveis, assim como 2 prefabs que podem ser copiadas e alteradas como desejado.

O exemplo é baseado no uso do pacote feito no jogo [FinalInferno](https://github.com/FellowshipOfTheGame/FinalInferno), onde herdamos as classes base Dialogue e OptionsDialogue para adicionar comportamentos adicionais ao iniciar e terminar um diálogo ao invés de pausar o jogo.

Se o efeito desejado é o do jogo [Anathema](https://github.com/FellowshipOfTheGame/anathema) as classes base devem ser suficiente, basta marcar a opção pauseDuringDialogue no DialogueHandler.
