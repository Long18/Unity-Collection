# Unity-Collection
This is my setting, function, etc. 


## Layout
- [Layout](Layout/layout.wlt)

#### *5 type*:
    - Tall
    - Wide
    - Dev
    - Editor
    - Noice

- [Script-NewBehaviourScript](Layout/81-C%23%20Script-NewBehaviourScript.cs.txt)

    - Get into: `C:\Program Files\Unity\Hub\Editor\2020.3.15f2\Editor\Data\Resources\ScriptTemplates` ***Example***



## Scripts
- [UI Camera Scaler](Scripts/UI/UICameraScaler.cs): For UI camera scaler
- [UI Scaler](Scripts/UI/FlexibleGridLayout.cs): For UI Scaler

- [Page Dot](Scripts/UI/Slide%20Controller/PageDot.cs): For tutorial slide dot
- [Slide Show](Scripts/UI/Slide%20Controller/SildeController.cs): For tutorial slide show or something else like that.
    
    #### - **Hierachy tree**:
        > Canvas
            > Game object (Slide Controller Script)
                > Panel
                    > Image ( For slide image - Page Dot Controller Script)
                    > Page Dot
                        > dot ( Image - Deactived)
                        > dotSpawner ( Horizontal Layout Group - Deactived)
                            > page ( Image )
                            > page 2 ( Image )
                            > page 3 ( Image )
                            > ...
            > Button (next)
            > Button (back)


- [Button Hover](Scripts/UI/Button%20Hover/ButtonHover.cs): For button hover

    #### - **Hierachy tree**:
            > DemoButton ( Button Hover Scripts )
                > outline ( Image - Turn the image off - Material:
                                                        buttonHoverOutline )
                    > Button ( Image - Button - Turn the button off & change
                                                    Transition: Sprite Swap )
                    > additive ( Image - Material: buttonHoverClick )