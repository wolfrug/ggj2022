INCLUDE ggj2022_functions.ink
INCLUDE ggj2022_puzzles.ink

VAR testVar = 1
{debug:
~debugReturnPlace = ->task7_mrsaveryTalk
->task7_mrsaveryTalk
}
==start
->task1_start

==inspectTest
This is an inspection. {Random1|Random2}

With many lines.

Very nice.
->DONE

==talkTest
You come across a  talking closet.

->Say("I am a talking closet.", Test)->
->Say("I like talking.",Test)->

You're not sure what to say.

+ [Cool.]
You think it's cool.
+ [Lame.]
You're too cool for talking closets.
- Either way. We're done here.
->Say("Byeee!", Test)->
->DONE

==stringtableTest
{&Random text 1|Random text 2|RandomText 3}

{&Random text 4|Random text 5|RandomText 6}

{&Random text 7|Random text 8|RandomText 9}
->DONE

==serumTest
You change into Hyde!
~isHyde = 1
->DONE