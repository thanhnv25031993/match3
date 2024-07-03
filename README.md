# Changelog

## v1.0.0

### Part 1:
1. Performing at least 3 changes can improve game performance.
- Optimizing textures by Sprite Atlas 
- Optimizing load game object from resource load to Addressable manager, for more details look for AddressableManager.cs file
- Optimizing spawn game object by Object pooling
- Add bomd delay time explosion after 0.5 second and then move down the item 

2. Use Scriptable Object to reskin all normal item textures (7 items) with new textures inside folder Textures/Fish.
- Look for the GameData folder and ItemSkinData data. This data handles switching from old sprites to new sprites (fish folder). I have attached the script to each item to load the sprite by ID in the data. 
- For switching between old and new, you just need to check or uncheck the "Use new skin" toggle in "ItemSkinData".
3. Create a restart button
- I have created a restart button in 2 places:
* Game Panel
* Game over panel
4. Edit the FillGapsWithNewItems function .....
- I have finished creating a new item in the FillGapsWithNewItems function by checking the 4 directions of the new item and also checking the type of item that has the least amount on the board. 
### Additional Notes:
- I was going to add an explosion effect to the bomb, but I don't think it's necessary
- I have been fixed some minor bug in this project (basically is about null or exeption)
- If the project is bigger, I will create a level design tool to make level design easier.
- Spawn items have a logical system with each other
- I must say, I don't like developing match-3 games
