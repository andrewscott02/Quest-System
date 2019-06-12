### Blank Unity Project  - Changelog - June 2019 ### 

* Updated project to use Unity 2018.4.2.f1.
* Removed out-of-date git plug-in.  If you want to use this feature, add it from The Asset Store.
* Removed unneeded packeges from the default project ( Ads, Analytics and Collaborate are not required for most projects.  Text Mesh Pro has been left in as that is regularly used in projects).
* Created barebones sample project folder structure (Scenes, Materials, Prefabs).
* Added a sample asset to each folder to ensure they are not empty.
* Forced deferred rendering throughout the project.
* Forced main camera to use deferred rendering and to not use MSAA.  (For antialiasing, add the Post Processing Stack v2 from The Package Manager).
* Disabled resolution / quality dialog from start up and set the resolution and quality defaults to the best defaults available (Default resolution is native resolution.  Display mode is Fullscreen borderless window.  All quality settings use deffered rendering, but are otherwise left as default).
* Changed the colour space from 'Gamma' to 'Linear'.  Linear colour space provides a better range of colours.
* Disabled Mixed Lighting and Autogenerate lighting.  Only realtime global illumination is used by default.  If you want to bake static lightmaps, enable mixed lighting in the lighting settings.
* Changed player settings to force a single instance, run in background and be visible in the background.  These are generally the best all round default settings for the player.


