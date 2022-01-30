// Puzzle ink file!

VAR task1_state = 0
VAR task2_state = 0
VAR task3_state = 0
VAR task4_state = 0
VAR task5_state = 0
VAR task6_state = 0
VAR task7_state = 0
VAR task8_state = 0

VAR debugReturnPlace = ->task3_conciergetalk
==debugTasks
+ [Get danvers clue]
{Add(clue_danvers, 1)}
->debugTasks
Adding the key
+ [Get the ramkin clue]
{Add(clue_ramkin, 1)}
->debugTasks
Adding note.
+ [Go to returnplace.]
->debugReturnPlace

==endtalk
{debug:
->debugTasks
}
->DONE

==task1_start
#focusOn.body
A prominent politician is dead, and the Constabulary want the situation resolved as quickly, and <i>quietly</i> as possible.

It seems Minister Blake was living a <i>double</i> life, taking illicit <b>serum</b> in order to metamorphose into another version of himself.

<i>{Knight} I would never have guessed he was a serum user from his politics, but I guess you never can tell with <b>Alters</b>.</i>

{Rook} You really can't, just look at the two of us.

<i>{Knight} That's right, look at us - an Inspector and his secret Alter. Just one bad decision away from a messy death in a low rent boarding house.</i>

#focusCamOn
The body lies before you, caught mid-transformation - bones twisted, flesh distorted, caught forever between two separate personas. A violent end.

These split personas are becoming more common, though public opinion still considers it indecent - perhaps even <i>criminal</i>, to engage in such behaviour.

For some <i>Purists</i>, the <b>serum</b> is the embodiment of evil, bringing out the darkest desires of mankind and driving them to deviancy.

For others, it is an escape a way to free oneself from the shackles of a rigid society and become your <b>true self</b>. 

<i>{Knight} It certainly was that way for us.</i>

Whatever one's opinion, a man lies dead, and it's <i>our</i> job to find out why.

Was it a tragic accident, or something more sinister?


#focusCamOff
~task1_state = 1
<i>{Knight} Let's take a turn around the room and see if there's anything useful.</i>

{debug:
->task1_examinebody
}
->endtalk

==task1_name
Examine the crime scene.
->DONE

==task1_description
I should look around for clues.
->DONE


==task1_examinebody
#focusOn.body
#focusCamOn
{You've seen your share of gore during your stint as a Detective for the Special Constables, but this body takes the cake.|What else?|Anything more?}

* [Examine his coat]
->coat
* [Examine his vest.]
->residue
* [Look through his pockets.]
->key
+ [Finish your examination.]
->endtalk

=coat
He's wearing a poor man's overcoat on top of his fancy MP suit - suggesting he wasn't accidentally changed, but did so on purpose. However, the fact he is still wearing his suit underneath also suggests he was in a hurry to transform for some reason.
{Add(clue_disguise,1)}
~task1_state++
{task1_state>=6:
->task1_finish
}
->task1_examinebody
=residue
There's a strange powder on his vest - the smell makes you feel extremely ill, and you quickly step away.
{Add(clue_residue,1)}
~task1_state++
{task1_state>=6:
->task1_finish
}
->task1_examinebody

=key
You find a key in his pocket, marked with the apartment's number. That means he must have gotten it from the concierge.
{Add(item_key,1)}
~task1_state++
{task1_state>=6:
->task1_finish
}
->task1_examinebody

==task1_examineroom

=serum
A broken vial containing transformation serum. The serum causes an individual to metamorphose into another person, bringing out their "inner self", otherwise known as an "alter".
{Add(item_brokenSerum, 1)}
~task1_state++
{task1_state>=6:
->task1_finish
}
->endtalk
=note
A crumpled note lies on the floor near the body - <b><i>"Room 2, 10 o'clock. - E.D."</i></b>. The note seems to have been written in haste, but the paper is thick and expensive, and smells faintly of dried flowers.
{Add(item_meetingNote, 1)}
~task1_state++
{task1_state>=6:
->task1_finish
}
->endtalk
=room //adjust this to fit with the art
{The room is sparse, but clean.|Cheap housing for a minister's alternate persona?|A meeting place for an illicit affair?|There isn't much here to go on.|Whoever lived here shall have to remain a mystery - for now.}
->DONE
=exit
Not yet. I haven't looked at everything.
->DONE

==task1_finish
#focusCamOff
There's nothing else in the room to examine.

The person who discovered the body was Mr. Avery from Room 6.
#focusOn.room1exit
#focusCamOn
Leave the room and proceed downstairs to interview the witness.

You could also talk to the Concierge about the room.

//maybe change this to speak with the policeman outside, who can then direct you to Avery "the first person on the scene", as well as the Concierge who called it in.
~task3_state = 1
~task2_state = 1
->endtalk

==task2_name
Interview Mr. Avery
->DONE
==task2_description
I should speak with the one who found the victim, Mr. Avery.

He should be in room 6, just down the hall.
->DONE

==task2_roomcomment
That's room six, I think.
->DONE

==task2_averytalk
#spawn.portrait.avery
The room is spartan but well kept. Mr Avery is clearly waiting for you, and seems unusually anxious.
{not firstTalk:
->firstTalk
}
->questions
=firstTalk
"Did you find out what happened to that...man?" He asks you, clearly using the term "man" in the only the loosest sense of the word.

{{ThoughtBubble("No prizes for figuring out how he feels about Serum users.","Though the sight of that corpse would be enough to shake anyone's nerve.","")}|}

* [{Rook} No need to worry yourself about that, sir.]
* [{Rook} The investigation is still ongoing.]

- Could you describe the circumstances that lead to you discovering the body?

"Well I was on my way back from work, the door was open, and the body was clearly visible."

{Rook} Do you usually return home this early?

"A meeting was cancelled. The storm, you see." He gestures to the wall where a window would ordinarily be, but we're below the level of the New Town down here, deep in the Vaults of the Betelgeuse Boarding House.

{Rook} How did the body appear when you first saw it?

"How did it appear?" For a moment, Avery looks almost outraged.

"Good gods, man, it appeared monstrous - like something had torn him apart from within. I know some of the lodgers keep odd hours but I had no idea there was one of <i>them</i> in the building."

{Knight} One of us.

"You know, a serum user." Avery adds, pushing his glasses up the bridge of his nose with a furtive glance. "My wife was equally shocked, I can assure you."

{Rook} "That seems an odd assertion to make. Was Mrs Avery with you at the time you discovered the corpse?"

"What? No. Of course not. Ridiculous assertion." He blusters. "I told her about it later, when I came back here.

{Knight} Me thinks the master doth protest too much.
->questions
=questions
* {Rook} Where is Mrs. Avery now?

"She was overcome with shock, and had to step away to recover. There's no need to disturb her Inspector. I assure you, she has no information to add."
->questions
* {HasItem(clue_danvers)} [Did you know the man who lived in apartment 6? Edward Danvers?]
Avery looks guilty - probably for having witheld that information -  but nonetheless tilts up his chin, imperious, for all that he is a man of meagre means. "We barely knew the man, and I think we've done quite enough to assist your investigation. Don't you?"

{Rook} I suppose you have, Mr Avery. Nonetheless, if I have any further questions I hope you will accomodate me. Good day.
->questions
+ [Leave him be.]
~task2_state = 2
->endtalk



==task3_name
Talk to the Concierge
->DONE
==task3_description
I have to find out who stays in Room 2.
->DONE

==task3_conciergetalk
#spawn.portrait.concierge
The concierge is standing by his desk, nervously wringing his hands.

{{Knight} A death on the premises is hardly good for business.|}

{"Ah, Inspector!" He lowers his voice. "How can I help you?"|He looks at you in askance.}
{task5_state>0 && task5_secretaryTalk<1:
{"The Secretary of the MP has arrived, Inspector." He nods at her, standing by the sofa. "I told her to speak to you."|}

{{Rook} Thank you. I shall.|}
{{Knight} Interesting.|}
}

+ {task8_state==1 && HasItem(clue_fourthfloor)} [I want to go to the fourth floor]
->fourthfloor
+ {HasItem(clue_ramkin)} [I wish to meet with Lady Ramkin]
->ramkin
* {HasItem(item_key)} [I found this on the body.]
->key
* [Who other than Mr Avery and yourself has seen the deceased?]
->witnesses
* {HasItem(clue_danvers)} [Tell me about Mr. Danvers]
->danvers
+ [Nothing right now.]
->endtalk

=fourthfloor
The Concierge looks confused. "There's nothing up there but construction, Inspector."

{Rook} Can you take me there or not?

"Of course, sir. Follow me..."
+ [Wait, not right now.]
"Very good." He returns to his desk.
->endtalk
+ [(Go to the 4th floor)]
#teleport.lab
You follow the Concierge up to the fourth floor, where he opens the door for you and then quickly closes it behind him.

You hear the sound of his footsteps receeding quickly. Hmm.
->endtalk

=ramkin
The Concierge nods. "Of course. You will need a special key to enter her apartments."

"Follow me, please."
+ [Wait, not right now.]
"Very good." He returns to his desk.
->endtalk
+ [(Go to the 5th floor)]
#teleport.ramkin
You follow the Concierge as he leads you to the stairs, holding a big keychain...
->endtalk

=key
The concierge takes the key and looks at it.

"Yes, this is the key to the room." He looks shocked. "You don't suppose…"

{Rook} Who rents room two?

"That would be Mr. Danvers. Edward Danvers." He says. "But surely…"

{Add(clue_danvers,1)}
// finished task 3, starting task 4
~task3_state = 2
~task4_state = 1
->task3_conciergetalk

=witnesses
"No-one, sir. Mr. Avery came straight to me, and after I had ascertained there was indeed a…body…in the room, we sent for you at once."

{Knight} He probably recognized the MP. Question is what he is going to do about it.

* [Please keep the identity of the deceased to yourself for now.]
He nods. "Of course. Of course. Police business."
* [Thank you.]
-{Knight} By tomorrow all the papers will know. I would bet on it.
->task3_conciergetalk

=danvers
The concierge shakes his head. "I don't take a habit of paying too much attention to the guests."

{Knight} He's full of it.

{Rook} That seems rather unconventional.

He shrugs. "You can ask the other guests about him, if you'd like." He points over his shoulder. "There are some in the lounge right now."
->task3_conciergetalk


==task4_name
Find out more about Danvers.
->DONE

==task4_description
Some of the other residents must have met Danvers.

Let's see if they have anything interesting to say. 
->DONE

==task4_professor
#spawn.portrait.professor
A regal gentleman with a hooked nose stands by the window, gazing out at the storm. As you approach he turns to greet you with a stern expression. 
{task4_professor<2:
->firstTalk
- else:
->questions
}
=questions
* {HasItem(clue_danvers) && not danvers} [I don't suppose you knew Edward Danvers, a resident here?]
->danvers

+ [Good day sir.]
->endtalk

=firstTalk
{Knight} Worried, or just of a naturally dour disposition?

It's too soon to say.

"You must be the Policeman, Rook was it?" He says, shaking your hand. It seems news travels fast in the Betelgeuse. 


* [Inspector, actually - and your name is?] 

-"Maxim Vogel, Doctor. But not the medical kind." Is that a flicker of humour you see in his eyes? 

Maybe he isn't as much of a starched collar as he appears - though speaking of collars, his coat has seen better days. 

There is something threadbare in his countenance. The demeanour of a man recently fallen on hard times. 

{Add(clue_hardtimes,1)}

"Terrible business, this death on the lower floors. Was it some kind of accident?" Vogel asks casually, almost distractedly, but his eyes never leave your face.

{Knight} He's fishing, don't bite.

You skirt the question.

* {HasItem(clue_danvers)} [It was a resident. One Edward Danvers, I don't suppose you knew him?]
->danvers

+ [Good day sir.]
->endtalk

=danvers

-"Danvers?" Vogel looks visibly perturbed, though he quickly conceals it. An interesting reaction on several levels. 

"I'm afraid I didn't know him well. Though we sometimes passed one another in the halls - our rooms are on the same floor."

* [What was your impression of the man?]

- Vogel shrugs. "He seemed…kind. A man of conviction.I am sorry to hear that he's dead."

{Add(clue_agoodman,1)}

* [Can you remember the topic of your last discussion?]
-"Parapsychology. It's what I taught at the University before…well. Let's just say that we both shared an interest in the subject."

{Knight} Parapsychology, of interest to someone of our inclinations? You don't say.


{Add(clue_parapsychology,1)}

"Well that should be all for now. If you think of anything else please let me know."

"Of course. Of course." Vogel says. As you turn to leave he's already gazing back out the window, his frown ever deeper than it was when you arrived.
~task4_state++
{task4_state>=3:
->task4_finish
}
->endtalk

==task4_artist
#spawn.portrait.artist
A louche young man stands near the centre of the room, fiddling with some charcoal sketches and several heavy books.
{->firstTalk|->questions}

=firstTalk

"Ah, an artist!" You declare, sneaking a peek at the papers over his shoulder. 

The young man laughs, "That's the idea, but my father would probably disagree.".

The sketches are good, but unfinished. The books on the other hand appear to be on the subject of advanced chemistry. A curious combination.

{Knight} This one is smarter than he looks, and he smells like money. Old money.

{Rook} “A spot of light reading?” You ask, gesturing to the chemistry manuals. 

“Oh these?” Temple blushes and looks away. “They aren’t mine, Avery lent them to me as a favour. There are some very interesting chapters in here about natural pigments.”

{Rook} “Mr. Avery? Is that so?”

Temple barks a sudden laugh, and slaps you on the shoulder. You’ve clearly tickled his funny bone. “Gods no, that clod couldn’t make head or tail of any of this. It’s the fair Mrs Avery who leant me these books. She’s a gem, truly.”

{Add(clue_chemistry,1)}

He straightens his shoulders, and quickly shuffles the papers together, concealing the the books from your view.

"I'm afraid we haven't been introduced." The artist says, remembering his manners. "Alastair Temple, at your service."

* [Inspector Rook, likewise.]
- He nods, unsurprised. 

"Ah, the Inspector. I figured you'd be making the rounds. Honestly this whole situation couldn't have come at a worse time. My father's been badgering me for months to go back to University." He gives a helpless shrug. 

"And now this death in the building, he might actually stop paying my lodging fees." He says, with an air of dejection.

<i>{Knight} It's a hard life, having everything handed to you on a silver platter.</i>

"I don't suppose we're in any danger though, are we Inspector? They are saying it might have been a murder." He asks, his momentary melancholy gone as quickly as a summer squall.

* [They?]
-Temple waves his hands vaguely at the ceiling. "Oh you know, the residents." He flashes a disarming smile, but his eyes remain deadly serious. 
->questions


=questions
* {HasItem(clue_danvers) && not danvers}  ["Well perhaps they could help me find more information on the deceased, Mr Danvers. I'm trying to paint a picture of his character."]
->danvers
* ["Have you seen anyone new around the building lately? Anything suspicious?]
->blake
+ [Bid your farewell.]
->endtalk

=blake
"Well now that you mention it, I <i>did</i> notice someone coming down from the fourth floor this morning. Supremely odd under the circumstances.

"And he dressed chap. I didn't see his face, but I thought for a moment that he was one of those political pundits always getting themselves in the paper." Temple shrugs.

* [What's on the fourth floor?]

"That's just the thing, there's nothing up there. It belongs to Ramkin, one of her abandoned renovation projects. The whole place is locked up tight as a barrel."

{Add(clue_fourthfloor,1)}

->questions

=danvers
 "Danvers? You don't say." Temple's expression doesn't so much as twitch.

{Knight} Nerves of steel.

"That's unfortunate, he seemed like a decent chap. And he was a night owl like myself." Temple's eyes light up as he remembers something. 

"You know, I thought he was a bit boring at first - too earnest. But during one of our late night conversations he let slip that he was involved in some sort of secret affair. Quite the dark horse as it turns out." 

* [Do you know who he was seeing?]
"Haven't the faintest idea. When I tried to prod him on the subject he clammed up and left. And now… well." Temple shrugs apologetically, and lapses into silence.

Then a conspiratorial glint enters his eye. "Though just between us, I did rather get the impression it was someone in the building."
{Add(clue_secretaffair,1)}

~task4_state++
{task4_state>=3:
->task4_finish
}
->questions

==task4_finish
This is probably all you will get out of them.

You best return to the lobby.
~task5_state = 1
~task4_state = 4
->endtalk

==task5_name
Speak with the Secretary
->DONE
==task5_description
The victim's Secretary is waiting in the lobby.

I should talk to her.
->DONE

==task5_secretaryTalk
#spawn.portrait.secretary
{"I demand to know where the MP is. I'm his secretary, he will be looking for me!" The young woman gives the Concierge an angry look.|}

{not blakeDead:The young woman does not look overly perturbed. No-one must have told her what happened yet.|The young woman is clearly overcome with emotions.} She does not speak with the Concierge, but stands in silence.

{{Knight}  This should be interesting.|}

{"Inspector? The Concierge told me to speak to you." She mainly looks annoyed. "I don't know why he is giving me the run-around…"|}

* [You are the Secretary to Mr. Blake, the MP?]
->blake
+ [(Leave her in peace)]
->endtalk


=blake
She nods empathetically. "As I have been trying to tell the good Concierge, yes." She looks around the lobby. "That's why I'm here - to see if he is quite ready."

{Knight} So his secretary knew?

* [Does Mr. Blake have an apartment here?]

She shakes her head. "Oh no, of course not. He was here on business."

* [What business did Mr. Blake have here?]

She suddenly becomes a bit more alert.

- {Rook} Please go on.

"Mr. Blake's private business is his private business, Inspector." She says after a moment, looking very prim.

{Knight} Best to just tell her or she'll give <i>us</i> the runaround.

* [I am sorry to be the one to tell you, miss, but Mr. Blake is deceased.]
->blakeDead

->task5_secretaryTalk

=blakeDead
{The Secretary goes white. "What? How? I saw him not two hours ago.|The secretary looks pale, close to tears.|You are not sure how many more questions she can take.}

* [Tell me about the last time you saw Mr. Blake]
"We were right here, in this lobby. Mr. Blake told me he had some further business and to go on without him." The Secretary looked positively shaken. "I can't believe it…"
->blakeDead
* {HasItem(clue_danvers)} [Are you familiar with one Mr. Danvers?]
"Who?" She frowned. "No, I've never heard of him."
->blakeDead
* [What business did Mr. Blake have here?]
->ramkin

=ramkin
"He was meeting Lady Ramkin about their shared interest in philanthropy." She said, dabbing at her eyes.  "They had often corresponded before, but never met."

{Knight} There we go.

{Add(clue_ramkin, 1)}

* [Does Lady Ramkin reside in this building?]
The Secretary nods, clearly only barely holding it together. "She has the whole fifth floor." She sniffs. "Not that I saw much of it - their meeting was private."
* [Were you present at the meeting?]
She shakes her head, her face contorting with effort to keep her composure. "It was a private meeting, and I was not needed."

"But it took place here, on the 5th floor." She adds, as an afterthought.

- As you consider asking her something more, she breaks down into tears.

{Rook} Madam…

{Knight} Leave her alone. We have what we need.
~task6_state = 1
~task5_state = 2
->endtalk



==task6_name
Speak with Lady Ramkin
->DONE
==task6_description
Lady Ramkin has information about her meeting with Minister Blake, I need to find out more.

I should speak to the Concierge about accessing the fifth floor.
->DONE

==task6_ramkinTalk
#spawn.portrait.ramkin
{finish>0:
Lady Ramkin has nothing further to say at this time, but who knows, perhaps something will turn up later.
->endtalk
}
Lady Ramkin herself is waiting for you by the door, a regal woman with dark skin and pearl studded ears. 

"Well Inspector, I'm a busy woman. What is it that you require?" Lady Ramkin asks, pinning you with a calculating stare.

{Knight} Right to point, like a dagger to the spleen.

{Rook} Well Lady Ramkin–

"It's Selena, please, no need to stand on ceremony. It wastes so much time." She says, her eyes rolling with disdain.

* [Er, of course. Selena.]
You cough awkwardly and try to gather your thoughts. 

{Rook} "You had a meeting earlier today with Minister Blake. I'd like to know what you discussed?"

"Blake?" She examines her fingernails, and sighs. "We discussed money, what else. That's all that any meeting is ever about, and anyone who tells you differently is lying"

->questions

=questions

* [Where did the meeting take place?] 
-"Downstairs," she says vaguely, "I own everything above the third floor." 
{Add(clue_fourthfloor, 1)}

* [How did the meeting go?]
- Her lips purse together, a small involuntary motion.

"Disappointing. I thought we shared a vision for the future, but it appears the Minister doesn't have the nerve to follow through."

Selena flicks an imaginary piece of lint from her silk skirts. "It could have been a beautiful partnership, but he rushed off in the middle of my...<i>demonstration</i>." 

She smiles to herself, as though at some inside joke.

{Add(clue_meeting, 1)}

* [Can you elaborate?]

- For a moment her gaze narrows in on you, a quick re-assessment - though to what end you can't be sure. 

"I could. But I won't. He did seem to be in an awful hurry though."

"Why do you ask? Has something happened?"

{Knight} How sweet, she's pretending that she doesn't know that we know that she knows. 

* ["Just doing my due dilligence. Establishing a timeline for everyone in the building."]->finish

=finish
She sniffs, arching an elegent eyebrow in your direction. 

"Yes, the dead fellow from the lower levels. An <i>Alter</i>, wasn't he?" He lips curl around the word with obvious distaste. 

"Well if you want to know what I think, it was only a matter of time. When you start letting that sort of filth into the building standards will inevitably slip." She adds, an icy edge to her voice that sends chills down your spine.

<i>{Knight} Oh no, a Purist. She thinks people like Blake are evil incarnate. People like us…</i>

{Add(clue_purist, 1)}

"We have to hold ourselves to a higher standard, Inspector." Lady Ramkin says, as she turns away. "You can think about that as you see yourself out."

Just like that, it appears the conversation is over.
~task6_state = 2
~task7_state = 1
->endtalk


==task7_name
Find Mrs Avery
->DONE
==task7_description
You still need to speak to Mrs Avery. 

By now she should be fully recovered and ready to talk. 
->DONE

==task7_mrsaveryTalk
#spawn.portrait.mrsavery
{not questions:
Mrs. Avery is unexpectedly lovely, even now while pale and drained from...grief? You hesitate, but no, your instincts are correct. This doesn't appear to be the the result of shock, or delicate nerves as Mr. Avery led you to believe. This woman is in mourning, and there's only one person she could be mourning for. 

{Knight} Well…perhaps two.
"Inspector." She greets you, dry eyed and composed.  You get the feeling she's been like this ever since she found the body. 

{Rook} "Mrs. Avery." You respond. 

{Rook} Perhaps it's best if you start. I find that the truth always works best.

"The truth?"  She chuckles, a dessicated sort of sound. "I wouldn't know where to begin.
}
->questions

=questions
{Mrs. Avery looks at you, red in the eyes.|Mrs. Avery waits for your next question, listless.}

* [How did you discover the body?] ->body
* [Were you aware of Edward Danvers nature?] ->nature
* [What was the nature of your relationship to Edward Danvers.]->relationship
* [Can you think of any reason why someone would wish to cause Danvers harm?] ->harm
+ [Thank you, that is all for now.]
{body:
~task7_state = 2
~task8_state = 1
}
->endtalk


=body
"The note, he sent it up with the Concierge. But by the time I arrived it was too late. Edward was already lying there."
{Consume(clue_secretaffair, 1)}
{Consume(item_meetingNote, 1)}
{Add(clue_averyaffair, 1)}
->questions

=nature
"No!" She looks like she's going to be sick. "I would have never! To think that one of those <i>things</i>...that I let – no. I had no idea." 

She shakes her head, as if to expel the very thought. 

{Rook} And what of Minister Blake? Did you have any idea that Danvers was his alter ego?

"Of course not, It's just absurd. "Mrs Avery suppresses an involuntary shudder. "To think that grubby little politician was inside of him? That they were part of the same whole? I can't even conceive of it."

"Everyone knows that the serum makes you worse. That it brings out your darkest deviancies, causing the soul to rot. But Edward," she hesitates,"he just wasn't like that. Not at all."
->questions

=relationship
"The nature of our relationship?" She laughs again, and somehow the sound is even worse than before. 

"I thought we were partners. I thought that we held the same ideals - believed in making a better world." Her eyes press shut as she steadies herself. 

{Rook} What about your husband?

"Victor?" She blinks at you, nonplussed. "Victor doesn't believe in anything. Not anymore. Not for a long time."
->questions

=harm
"Edward was a kind, decent human being who cared deeply for the well being of others. If someone wanted him dead. Inspector, it was not for reasons relating to his character."

Mrs Avery sounds very certain. 

{Knight} Except for the husband of course.
->questions

VAR task8_examprogress = 0
==task8_name
Go to the 4th floor
->DONE

==task8_description
The Concierge will be able to get me there.
->DONE

==task8_fourthfloor
{Rook} "A secret laboratory."
<i>{Knight} One run by Purists.</i>

The thought is enough to send a shiver down your spine. This must be the source of that residue from the body.

{Rook} "Some sort of anti-serum? One that specifically targets Alters?"

<i>{Knight} Tread carefully, and try not to get us killed.

{Rook} “I will.”
->endtalk


==task8_examinelab

* [Examine the room]
->room
* [Examine the equipment.]
->equipment
* [Look through the cabinets.]
->cabinets
+ [Finish your examination.]
->exit
->endtalk

=room
#focusOn.labfloor
#focusCamOn
A small stained glass window is set into the far wall, giving you a glimpse of the foul weather outside. 

The walls are covered in diagrams and notations - glass fronted cabinets and laden tables stacked with all manner of unsettling specimens and tools. 

Massive vats of bubbling liquid line the one wall, and there is some sort of secure lockbox recessed into the centre of the floor.

{Knight} Looks more like a cage to me?

You lean in for a closer look through the bars, and see a small bare room below with a single chair bolted to the floor. 

You can see the edges of another door that must connect somehow to an apartment on the floor below.
~task8_examprogress++
{task8_examprogress>=3:
->task8_finish
}
->DONE

=equipment
The lab is decked out with all manner of expensive equipment, with large vats of various liquids simmering on various burners at low heat.


It must take an impressive array of chemicals to keep a place like this going.

{Knight} And an impressive amount of money.
~task8_examprogress++
{task8_examprogress>=3:
->task8_finish
}
->DONE

=cabinets
The cabinets are filled with all manner of dangerous chemicals and ingredients, and half of the flasks bear rather ominous “POISON” warnings on the labels. 

Whatever they are making in here, it isn’t candy.
~task8_examprogress++
{task8_examprogress>=3:
->task8_finish
}
->DONE

=exit
Not yet. There’s still more to see.
->DONE

==task8_finish
#focusCamOff
There's nothing left to examine.
~task8_state = 2
->task8_theories

==task8_theories
{Rook} So, we have one secret lab, a bunch of Purists in the building, a possible case of anti-serum poisoning, a bunch of people with an unusual interest in chemistry and an affair. 

Our victim meets with Ramkin as Blake, that's all established, then he wants to meet with his lover, Mrs. Avery, so he switches to his alter form.

During their meeting, they have a disagreement, or someone bursts in on them - and he's exposed to that residue we found. The transformation then kills him.

*[{Knight} No, no. That’s all wrong.]

-{Rook} Enlighten me then.

{Knight} Something happens during the meeting to alarm the victim, he hurries to meet with Mrs. Avery, his lover, putting on a coat and then drinking the serum.

But something goes wrong, and whatever he drunk kills him instead of turning him into his alter. Mrs. Avery then finds the body.

{Rook} Danvers was already in his alter form when he died - why would he have put on a coat before drinking the serum?

{Knight} He was in a hurry, that's why. He came to his room, put on the coat, and drank the serum.

* [{Rook} So you’re saying the Purists killed him, but accidentally?]

-{Knight} Precisely. And <i>you’re</i> saying that he was killed by his lover?

{Rook} Yes! But at the behest of Lady Ramkin. When their meeting went south she must have realised he was a serum user.

She would have wanted to keep a lid on everything and make sure Blake wouldn’t expose her plans.

{Knight} Covering her tracks. She certainly seems capable of it. And yet…

* [{Knight} Fine, have it your way.]->rook
* [{Rook} Alright, I can see the logic in your argument.]->knight

=knight

<i>{Knight} I think Avery is our mysterious Chemist, and perhaps she was at this meeting with Blake and Lady Ramkin. </i>

<i>{Knight} It’s possible he didn’t know she was involved, or maybe the sight of their “demonstration” rattled him, and he wanted to get away.  </i>

<i>{Knight} But first, he had to meet with the one person in the Betelgeuse who truly knew him, and somehow that led to his demise.</i>


{Rook} Looks like we need to have another chat with Mrs Avery.
->endtalk
=rook

{Rook} Avery’s one of the Purists. If she realised what he was, and that he’d been lying to her for all this time, well – let’s just say men have been killed for less. 

{Rook}But she’s just a lackey in all this. The real culprit is Lady Ramkin, and her political agenda. If her anti-serum gets onto the streets it could do untold harm.

<i>{Knight} Well then, let’s nab ourselves a murderess.</i>
->endtalk

==transformation
{transformation<2:
#spawn.background.black
//#playmusic.tense
As you turn to leave, there is a sound of smashing glass, and then the door slams shut, plunging the room into total darkness!
{VoiceClip("event_glassBreak1")}

{Rook} Can you smell that? There’s some sort of smoke…

{Knight} Rook, you massive oaf, don’t breathe it in! It must be the Antidote.

{Rook} Knight! It’s too late, I can feel it.

{Knight} No. Please, no. Rook!

{Knight} Rook?

{Knight} Don’t leave me behind…

{VoiceClip("event_transform")}
//transition happens, etc
~isHyde = 1

{Knight} “R-rook? Are you there? Oh, shit. I’m not dead. And I’m…me”
#changebackground

{Knight} The antidote must only be fatal if you attempt to use the serum soon after exposure. 
->endtalk
- else:
#teleport.rooftop
{Knight} Time to go end this...
#focusOn.suspect
#focusCamOn
{task8_theories.rook:
->confrontavery
- else:
->confrontramkin
}
}

==confrontramkin
#spawn.portrait.ramkin
Rook believed that Lady Ramkin was the true danger, and I owe it to him to see through his final wish. 

I just wish I didn’t have to do it on my own. Together we were strong. Whole, the person we were always meant to be. But now…

I am a shadow of myself.

You find Ramkin on the rooftop, a sharp silhouette cut against the backdrop of a stormy skyline. Ramkin looks like she was waiting for your arrival, though you see a flicker of surprise cross her features as she takes in your appearance.

“How disappointing.” She says. “I’d thought you were of finer character…Inspector.” Her mouth curls back in a cruel smile. 

“But just think of, now you are cured. Liberated from the shackles of your affliction. You ought to thank me, really.”

Cured? Seriously?

It takes every ounce of your composure not to punch her in her smug face.

{Knight} “Why?” You ask her. A simple enough question in the face of everything that’s happened. “Why did you do all of this? You killed Blake and Danvers.”

{Knight} “You killed Rook.”

{Knight} “So what was it all for?”

Lady Ramkin graces you with a beatific smile. “Why, for the best cause of all. Truth.”

“To keep us free from sin, to keep us pure, and deliver us from that which would corrupt us, Miss… Oh, how remiss of me, I don’t even know your name.”

{Knight} It’s Rook. I’ts always been Rook, deep down. And you stole that from me. You talk to me about truth, but you stole my truth from me the minute you locked me in there with that antidote.”

Ramkin waves her hand dismissively.  “Oh I didn’t bother with that. That was Abernathy, the Concierge. He had a hunch, and wanted to make sure you didn’t get in the way. I do loathe an over enthusiastic employee, but Avery no longer has her heart in the work, and I’m short of hands.”

{Knight} “So what’s your big plan then, floor the streets with antidote? Kill us all?”

“Not <i>kill</i>. How goache. No. <i>Cleanse</i>. I will cleanse this city of its afflictions, by force if necessary.

“And now that the Inspector is gone, well, no one is going to listen to a mere slip of a girl.” Ramkin sniffs imperiously. “I suppose I really ought to thank Abernathy after all.”

You start to laugh, and longer it goes on, the more worried Ramkin appears.

{Knight}“Oh, how precious. You think the constabulary don’t know what I am? That they didn’t hire me <i>specifically</i> for my particular talents?”

*[You take your Inspectors badge out of your coat pocket.]
-

{Knight} “Lady Ramkin, I’m placing you under arrest for the killing of Henry Blake, and Edward Danvers.
->task9_theend

==confrontavery
#spawn.portrait.mrsavery
///I am not above reusing sections of text for each ending. Tailoring be damned.

Mrs Avery holds the key to all my questions, and it’s time to find out the truth. Rook would have wanted me to see this through to the end.

I just wish I didn’t have to do it on my own. Together we were strong. Whole, the person we were always meant to be. But now…

I am a shadow of myself.

You find Avery on the rooftop, a distraught figure cut against the backdrop of a stormy skyline. 

She stands at the edge, like a person poised to jump, and when she sees you a flicker of surprise crosses her features as she takes in your appearance.
“You’re a liar, just like Edward.” She says, her voice full of sorrow. “I don’t know what to believe anymore, or who.”

{Knight} “And you think this will help?” You gesture to the edge of the building where the rain overflows the spigots, falling to the dark streets below.

“Maybe. I can’t see a future. We were meant to leave, to get away and start something better.”

{Knight} “You and Danvers?”

“I honestly thought that people like him – people like you,’ she flashes you a sidelong glance, ‘were evil. It seems I was a fool about a lot of things.”

{Knight} “How did Danvers die?”

She turns away, stifling tears. “It was at the meeting, During the demonstration, Blade would have inhaled the fumes.” 

“The anti-serum isn’t lethal on it’s own. We aren’t trying to kill people. It’s meant to be a cure, a salve to heal the wounds of our twisted society.”

{Knight} “But at the meeting Blake saw you, realised who you were working for. He turned into Danvers so he could confront you about it.”

“The fool, if he’d only waited. If he’d just explained. Maybe everything would have been alright. It was all just a stupid accident”

{Knight} And what about me? What about Rook? Is he gone for good?”

Avery shrugs, apologetic. “I honestly don’t know Inspector. I’m sorry, I panicked, I saw you muttering to yourself in the lab, and you were figuring out all our secrets and I just — I’m sorry.”

“Perhaps with time he will come back. I can’t say.”

Swallowing your grief, for a moment you consider leaving her to her purpose. Let her fling herself from the rooftop and all this comes to an end. 

Except then without a witness to the whole sticky mess, Ramkin remains free to spread her poison.

{Knight} “Rook would have wanted justice. For everyone. Do you want to help me deliver that justice, Mrs Avery? Or will you let it all be for nothing.”

*[You take the Inspectors badge out of your coat pocket.]
-

{Knight} “Come, Mrs Avery. It’s time to go to the station. Help me put this right.”
->task9_theend

==task9_theend
#WinGame
This is the end.

->endtalk



