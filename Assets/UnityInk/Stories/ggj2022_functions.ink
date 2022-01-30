//Some default functions for the InkWriter

VAR debug = true
VAR lastSavedString = ""
VAR lastSavedTags = ""
VAR sayStarted = false

// This is used to flip avatars and such in the world, important!
VAR isHyde = 0

VAR died = false

VAR checkItem = -1

LIST characters = Test, Hyde, Jekyll

LIST items = item_none, clue_disguise, clue_residue, item_key, item_meetingNote, item_brokenSerum, clue_danvers, clue_hardtimes, clue_agoodman, clue_parapsychology, clue_secretaffair, clue_ramkin, clue_chemistry, clue_purist, clue_meeting, clue_averyaffair, clue_fourthfloor

VAR debugInventory = item_none

VAR Rook = "<color=blue>Rook:</color>"
VAR Knight = "<color=green>Knight:</color>"

VAR IM = "\[useText.PlayerInnerThoughtsText]"

VAR innerMonologueTextObject = "PlayerInnerThoughtsText"

VAR portraitTextObjectName = "CharacterNameText"

VAR useItemNumber = 0

EXTERNAL _CheckHasItem(x,y)
EXTERNAL _ConsumeItem(x,y,z)
EXTERNAL _AddItem(x,y,z)
EXTERNAL _VoiceClip(x,y)
EXTERNAL _ThoughtBubble(x,y,z,q)

==function Consume(item, amount)==
{CheckItem(item, amount)>=amount:
~useItemNumber++
{_ConsumeItem(ConvertToString(item), amount, useItemNumber)}
~debugInventory-=item
}
===function Add(item, amount)===
~useItemNumber++
{debug:
~debugInventory += item
}
{_AddItem(ConvertToString(item), amount, useItemNumber)}
==function _AddItem(item, amount, itemnumber)===
[Adding {amount} {item}]

==function CheckItem(item, amount)==
// Helper function
{debug && debugInventory?item:
~checkItem = 1
- else:
~checkItem = 0
}
{_CheckHasItem(ConvertToString(item), "checkItem")}
~return checkItem

===function _ConsumeItem(itemName, amount, itemnumber)===
(Attempted consumption of {amount} {itemName})

===function _CheckHasItem(itemName, returnVar)===
[Checking if we have item by name {itemName}]

==function HasItem(itemName)===
~return CheckItem(itemName, "checkItem")>0

===function VoiceClip(id)===
~useItemNumber++
{_VoiceClip(id, useItemNumber)}

===function _VoiceClip(id, itemnumber)===
[Play Voice clip {id}]

===function _ThoughtBubble(x,y,z,q)===
TB: {x}
{y !="":TB: {y}}
{z !="":TB: {z}}

===function ThoughtBubble(x,y,z)===
~useItemNumber++
{_ThoughtBubble(x,y,z, useItemNumber)}

===function ConvertToString(targetItem)===
// Add more items to this list as needed
~temp returnVar = item_none
{targetItem:
- clue_disguise:
~returnVar = "clue_disguise"
- clue_residue:
~returnVar = "clue_residue"
- item_key:
~returnVar = "item_key"
- item_meetingNote:
~returnVar = "item_meetingNote"
- item_brokenSerum:
~returnVar = "item_brokenSerum"
- clue_danvers:
~returnVar = "clue_danvers"
- clue_agoodman:
~returnVar = "clue_agoodman"
- clue_hardtimes:
~returnVar = "clue_hardtimes"
- clue_parapsychology:
~returnVar = "clue_parapsychology"
- clue_secretaffair:
~returnVar = "clue_secretaffair"
- clue_chemistry:
~returnVar = "clue_chemistry"
- clue_ramkin:
~returnVar = "clue_ramkin"
- clue_purist:
~returnVar = "clue_purist"
- clue_meeting:
~returnVar = "clue_meeting"
- clue_averyaffair:
~returnVar = "clue_averyaffair"
- clue_fourthfloor:
~returnVar = "clue_fourthfloor"
}
// and return
~return returnVar

==function ConvertToItem(targetItemString)===
~temp returnVar = item_none
~return returnVar

===function UseButton(buttonName)===
<>{not debug:
\[useButton.{buttonName}]
}
===function DisableButton()===
<>{not debug:
\[disable\]
}
===function UseText(textName)===
<>{not debug:
\[useText.{textName}]
}

===function Inner()===
{UseText(innerMonologueTextObject)}

===function ReqS(currentAmount, requiredAmount, customString)===
// used to enable/disable options [{Req(stuffYouNeed, 10, "Stuffs")}!]
{currentAmount>=requiredAmount:<color=green>|<color=red>}
<>{not debug:
\[{currentAmount}/{requiredAmount}\ {customString}]</color>
- else:
({currentAmount}/{requiredAmount} {customString})</color>
}

// convenience function that assumes min 0 and max 1000 on any value
===function alter(ref value, change)===

// if you need to alter values of things outside of checks, use this instead of directly changing them
// use (variable, change (can be negative), minimum (0) maximum (1000...or more).
{alterValue(value, change, -10000, 10000, value)}

===function alterValue(ref value, newvalue, min, max, ref valueN) ===
~temp finalValue = value + newvalue
~temp change = newvalue
{finalValue > max:
{value !=max: 
    ~change = finalValue-max
- else:
    ~change = 0
}
    ~value = max
- else: 
    {finalValue < min:
    ~change = -value
    ~value = min
- else:
    ~value = value + newvalue
    }
}
~temp changePositive = change*-1
{change!=0:
#autoContinue
{change > 0:
        <i><color=yellow>Gained {print_num(change)} {print_var(valueN, change, false)}.</color></i>
    -else:
        <i><color=yellow>Lost {print_num(changePositive)} {print_var(valueN, change, false)}.</color></i>
}
}

// prints a var, capital, non capital, plural or singular
==function print_var(ref varN, amount, capital)==
{amount<0:
// Make amount always positive, in case it's a negative amount.
~amount = amount * -1
}
{varN:
-"AnyString":
{amount==1:
    {capital:
    ~return "Anystring"
    - else:
    ~return "anystring"
    }
- else:
    {capital:
    ~return "Anystrings"
    - else:
    ~return "anystrings"
    }
}
}

===function StartSay()===
#startSay
~sayStarted = true
<i></i>

===function EndSay()===
#endSay #changeportrait
~sayStarted = false
<b></b>

==Say(text, character)==
// ->Say("Text", character)->
{not sayStarted: {StartSay()}}
#changeportrait

{character:
- Test:
#spawn.portrait.example
}
//{character!="": {character}: {text}|<i>{text}</i>}
{character}: {text}
{EndSay()}
->->

// prints a number as pretty text
=== function print_num(x) ===
// print_num(45) -> forty-five
{ 
    - x >= 1000:
        {print_num(x / 1000)} thousand { x mod 1000 > 0:{print_num(x mod 1000)}}
    - x >= 100:
        {print_num(x / 100)} hundred { x mod 100 > 0:and {print_num(x mod 100)}}
    - x == 0:
        zero
    - else:
        { x >= 20:
            { x / 10:
                - 2: twenty
                - 3: thirty
                - 4: forty
                - 5: fifty
                - 6: sixty
                - 7: seventy
                - 8: eighty
                - 9: ninety
            }
            { x mod 10 > 0:<>-<>}
        }
        { x < 10 || x > 20:
            { x mod 10:
                - 1: one
                - 2: two
                - 3: three
                - 4: four        
                - 5: five
                - 6: six
                - 7: seven
                - 8: eight
                - 9: nine
            }
        - else:     
            { x:
                - 10: ten
                - 11: eleven       
                - 12: twelve
                - 13: thirteen
                - 14: fourteen
                - 15: fifteen
                - 16: sixteen      
                - 17: seventeen
                - 18: eighteen
                - 19: nineteen
            }
        }
}
// prints a number as pretty text but with a capital first letter
=== function print_Num(x) ===
// print_num(45) -> forty-five
{ 
    - x >= 1000:
        {print_num(x / 1000)} thousand { x mod 1000 > 0:{print_num(x mod 1000)}}
    - x >= 100:
        {print_num(x / 100)} hundred { x mod 100 > 0:and {print_num(x mod 100)}}
    - x == 0:
        zero
    - else:
        { x >= 20:
            { x / 10:
                - 2: Twenty
                - 3: Thirty
                - 4: Forty
                - 5: Fifty
                - 6: Sixty
                - 7: Seventy
                - 8: Eighty
                - 9: Ninety
            }
            { x mod 10 > 0:<>-<>}
        }
        { x < 10 || x > 20:
            { x mod 10:
                - 1: One
                - 2: Two
                - 3: Three
                - 4: Four        
                - 5: Five
                - 6: Six
                - 7: Seven
                - 8: Eight
                - 9: Nine
            }
        - else:     
            { x:
                - 10: Ten
                - 11: Eleven       
                - 12: Twelve
                - 13: Thirteen
                - 14: Fourteen
                - 15: Fifteen
                - 16: Sixteen      
                - 17: Seventeen
                - 18: Eighteen
                - 19: Nineteen
            }
        }
}