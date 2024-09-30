# Mobile flower hangman word game
The third project in the MakeYourGame online XR App developper course is to make a wordgame application.  
This is the state of the project on the day of submission but will be updated sporadically on my own time


### The concept of the game
I consider Hangman to be an educational game, often played in school and by children/teens. With this in mind I never understood why the imagery of the game is a hanged person.
<br>
It is for this reason and in order to bring a more delightful version of this classic game that I chose the theme of flowers. In order to bring even more educational value I've decided to only focus on plants as the words to guess, while also giving pertinent hints and a small description of the plant at the end.
> Example of the end screen
![The main menu when entering the game](/gitAssets/end-description.png)

<br>

## DOTween and USS animations
In order to facilitate the use of DOTween with UIToolkit elements I created an extension class to be able to use shorthand methods for VisualElements
<details>
<summary>Click to see some of the animations I made for this project</summary>
  
> The restart curtains
![A curtain closing and opening when restarting](/gitAssets/curtains.gif)

> The petals falling on a wrong guess and the sunshine on a correct one
![A petal falls when the letter is wrong and a sunshine glows behind the flower when it is right](/gitAssets/letter-press.gif)

> A simple transition using USS for the statistics
![A transition with easing from the leaderboard to stats page](/gitAssets/stats.gif)

> The Match 3 animations [WIP]
![The game screen](/gitAssets/match3.gif)

</details>


## Figma blockout
This is the original Figma blockout made when creating the design documents for the game
<details>
<summary>Click to see the figma blockout design</summary>
  
> The main menu screen
![The main menu when entering the game](/gitAssets/hangflower_main.png)

> The settings screen
![The settings screen after pressing settings](/gitAssets/hangflower_setting.png)

> The mode selection screen
![The mode selection after pressing play](/gitAssets/hangflower_playmode.png)

> The game screen
![The game screen](/gitAssets/hangflower_game.png)

> The ending screen
![The ending screen](/gitAssets/hangflower_end.png)

</details>

## Class Diagram

<picture>
 <source media="(prefers-color-scheme: dark)" srcset="/gitAssets/MYG-Project-3-dark.drawio.png">
 <source media="(prefers-color-scheme: light)" srcset="/gitAssets/MYG-Project-3.drawio.png">
 <img alt="Game class diagram made with draw.io" src="/gitAssets/MYG-Project-3-dark.drawio.png">
</picture>

<br>

# The features of the game

<br>
 
- PlayFab Integration:
    -
    This project uses playfab to handle user authentication (currently a simple non recoverable system with deviceID) and setting a display name. The words to find in the game are stored as a json array stored in TitleData and when users log into the game they compare their version of that json in their persistent data with the playfab version and update it if necessary. PlayerData is used to store the win/loss stats of each user and also serves to display the ingame leaderboard.

- UIToolkit:
    -
    All the game except the match3 board is made using UI toolkit. This posed some challenges notably for changing some default unity elements like radio buttons or sliders, but more so when trying to animate VisualElements with DOTween. For that reason I created a DOTween extension class with static Tweeners adapted to VisualElements for the Tweens I used the most like DOScale or DOMove. This allowed me to keep portions of my code cleaner by using these shorthand expressions when building DOTween sequences for example.
- Scriptable Objects
    -
    For objects that purely store data I went for a structure that uses scriptable objects, this includes my words which are comprised of : 
    1. Their common and scientific names with their respective length
    2. A hint with a colored emphasized word reflecting the plant to guess
    3. A short description that is shown at the end of the game
    4. A category
    
    The BGM and SFX are also scriptable objects and the audio manager that works on the Observer pattern instantiates prefabs and give them the corresponding audio_SO when invoked.
- Expandable Match 3
    -
    While the match 3 mode of the game is not complete, I've built it with expansion in mind. As such it is simple to add rows or columns to the board, change or add new item types or code in new powerups that affect the board or points. The regions and comments to the methods that are as separated by function as can be also make it easy to navigate and change code on the fly.

# Work in Progress
As a part of a complexification of the project I worked on a match 3 prototype. All the current features are working so far but it is missing some polish, additional power ups and a proper Win/Lose implementation. I also need to figure out a system to fetch words and set the progress bar values in a balanced way.

> [!IMPORTANT]
> You can try the app on my itch.io page and download it (android only) [Flowery Journey](https://babadulnek.itch.io/flowery-journey)
