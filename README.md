# UpdateLog

This is a Modder Tool that allows you to create a dialog that shows when launching the game.

To use this tool you should add the reference to your project via Nuget Package Manager. It's the quickest and easiest way to add the reference to the project. If you don't want to do it this way, then clone the repo and only copy over the UpdateLog.dll file into your Assemblies folder.

If you still have questions you can drop by the [Discord Channel](https://discord.gg/zXDyfWQ) and ask there.

### Dependencies
This mod does not have any "mod" dependencies in the sense that you would need to add the dependency to your own mod. It does however use Harmony to avoid using any components from RimWorld which means this tool may be added / removed from existing projects with no worry of causing save file issues.

### Getting Started
Once you have the .dll referenced, you can now set up your UpdateLog. The UpdateLog uses the *Current Version* of RimWorld like normal projects do. This means if your project has both 1.1 and 1.2 folders, including the UpdateLog in 1.2 will only provide the update log for 1.2. This prevents showing the update dialog for other versions.

Create a folder in the appropriate Version folder (ie. 1.2) and name it UpdateLog. Inside this folder you'll want to include an .xml file named `UpdateLog.xml`  
This file will be where you provide the necessary information to push an update dialog next time you are updating your mod on steam. Alternatively you can leave the folder empty and the `UpdateLog.xml` file will be generated for you when you run the game. After running the game once, your file will be written into with all fields that you can utilize as well as some comments to give you a basic understanding of what each field does.

**Images**  
If you plan on showing images or icons inside the UpdateLog dialog window you'll also want to create a folder named `Images` inside the `UpdateLog` folder which will contain all of the textures used inside UpdateLog.  

**UpdateLog.xml**  

Quick Note:  
Due to the way xml handles the < > & ' and " characters, you cannot directly type them in in your UpdateLog.xml file. For the angle brackets < and > you can use square brackets \[ and \] which will be replaced with the angle brackets when being parsed in. I did this solely for simplicity sake. If there is a conflicting url with square brackets in it please let me know.  Alternatively you can use the special characters which will be swapped when the xml document is built in code. These are...  
`&lt;` for `<`  
`&gt;` for `>`  
`&amp;` for `&`  
`&apos;` for `'`  
`&quot;` for `"`  

**Fields**  
`<currentVersion></currentVersion>`  
The current version of the mod. This accepts a string so input whatever you like although the format for versions is *generally* x.x.x for major, minor, and patch versions.

eg.
```xml
<currentVersion>1.3.1092</currentVersion>
```

`<updateOn></updateOn>`  
This determines when your UpdateLog.xml file will be checked for a new update and subsequently displayed. There are 4 options:  
Startup, GameInit, LoadedGame, and NewGame.  

`Startup` will display after all mods are fully loaded but right before the main menu is drawn.  
`GameInit` will display after a game is initialized. *This applies to both existing and new games.*  
`LoadedGame` will display after an existing game is initialized. *This only applies existing games.*  
`NewGame` will display after a new game is initialized. *This only applies new games.*  

`<description></description>`  
The text to be displayed in the update dialog window when a new version is registered. You may enter certain brackets to format your text or insert things like links and images. The list of brackets can be found [here](link go here)  
If you leave this field blank the dialog window will not be displayed even if a new version is registered.

`<actionOnUpdate></actionOnUpdate>`  
This will call a method right before the dialog window is opened. The method must be static and contain no parameters.  
For the field input in the format: Namespace.ClassName.MethodName  
If you leave this field blank, it will bypass this part.

`<rightIconBar></rightIconBar>` and `<leftIconBar></leftIconBar>`  
These 2 fields will create clickable icons that will open a webpage from the url provided.  
Use the left or right IconBar field to add these links. They will show up starting from the outside working their way in.

These 2 fields are lists containing 3 additional fields... the `name`, `icon`, and `url` so it will look like:  
```xml
<rightIconBar>
    <li>
      <name>Github</name> <!-- Name that will appear when Mouse is hovering over the icon -->
      <icon>githubIcon</icon> <!-- Name of the image file so that the cached texture can be retrieved. The image will need to be in the Images folder -->
      <url>https://github.com/SmashPhil/UpdateLog</url> <!-- url link navigated to when the icon is clicked -->
    </li>
</rightIconBar>`
```

If you only want to show the icon you can leave the url field blank. This will generate the icon with no MouseOver color or click event.

Note: These icons are fitted to a specific square so it is highly recommended you use an image with equal dimensions ie. 250x250  

**Updating**  
When you're ready to update there is an additional field in `UpdateLog.xml` named `<update></update>`. Input `true` and the next time your mod is loaded it will register there is a new update for this specific Mod.  

**Testing**  
If you would merely like to test your update and see what the dialog window would look like with specific formatting you can set the field: `<testing></testing>` to `true` which will prevent the UpdateLog file from being written into and removing the update tag. This means if you have `<update/>` set to true it will remain as such even after your dialog is shown.  

**Version.txt**  
If you would like your current version to automatically update alongside your project, you can include a Version.txt file in the root directory of your mod and it will pull whatever version is in that file rather than using the version in the UpdateLog file. When it writes over the file on restart it will update the current version.  

*Sounds like extra work to me...*  
Not if you pair this with some build setting or line in a static constructor that writes to this file with your current AssemblyVersion. I use this exact method in Vehicles to keep the UpdateLog and my project's assembly version in sync. It's fast, easy, and most of all requires less work to push out an update.

## Tags
For a full comprehensive list of tags you can use within the description, read [here](https://github.com/SmashPhil/UpdateLog/wiki)

If you would like to create your own tags to use within the description, read [here](https://github.com/SmashPhil/UpdateLog/wiki/Creating-A-Custom-Bracket)
