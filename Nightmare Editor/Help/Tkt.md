# Using the Romhacking Suite
 This Romhacking Suite is what this program is mostly based around. Even if you've used it before, it's used a bit differently here. I'll specify what selection to use each time. It's only different depending on the button you pressed before it was opened.
![Tkt1](Help/Images/Tkt1.png)
#### 
## Upon Clicking the Add File (+) Button:
### If you selected an .rbin file...
 Type '1' and press enter. Once the window says "Done!", press any key.
#### 
### If you selected a .ctt file...*
 Type '12' and press enter. Once the window says "Done!", press any key. The file will be added inside 'User-Added.rbin'.
#### 
### If you selected a .pmo, .pmp, .l2d, or fep file...*
 Type '14' and press enter. Once the window says "Done!", press any key.
#### 
## Upon Clicking "Replace"...
 This is functionally the same as the Add File button, at least, in terms of what keys to press. Refer to the above section.
#### 
## Upon Clicking "Re-extract":
 Type '1' and press enter. Once the window says "Done!", press any key.
#### 
## Upon Clicking "Pack"...
### If you selected a .ctt file...
 Ok so nothing will happen here, but I do want to talk for a bit, and this is my program, you can't tell me what to do! >:)
 Anyway, originally you would've pressed 13 and enter, then 12 and enter. However, the packing in the toolkit just *didn't work*. It basically just returned a blank image. So I spent almost the entire next day implementing decoding/encoding ctt textures from scratch. Within that day, I was able to completely implement RGBA8888, RGB888, and RGB565. I also adjusted many minor things, like switching to the correct format identifiers, and implementing a half-baked texture linking that didn't store your progress, unlike the final version. Anyway, thanks for listening! Moving on...
#### 
### If you selected a .pmo, .pmp, .l2d, or fep file...*
 Type '14' and press enter. Once the window says "Done!", press any key.
#### 
## Upon Clicking "Pack Queued"...
 Take a look at the textbox in the top-left of the main window. It will state what file is currently being packed.
#### 
### If the textbox says an .rbin file...
 Type '2' and press enter. Once the window says "Done!", press any key.
### 
## If marked with an *, it's possible the Romhacking Suite window may not appear.