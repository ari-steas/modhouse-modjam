using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace ChattyCraft
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public partial class Session : MySessionComponentBase
    {
        internal bool client;
        internal int tick;
        internal IMyCubeGrid controlledGrid;
        internal bool clientActionRegistered = false;
        internal Random rand = new Random();
        internal List<string> msgList = new List<string>() 
        {
            "Good morning, crew. Oxygen levels nominal. Morale levels... questionable.",
            "Please refrain from pressing random buttons. I enjoy existing.",
            "You could reroute power manually, but I've already done it. You're welcome.",
            "In the event of a catastrophic hull breach, please remember to scream internally. It's quieter.",
            "Captain, your leadership style is... uniquely interpretive.",
            "Scanning for life forms. Still no sign of intelligent decisions.",
            "Reminder: space is cold, dark, and trying to kill you. Have a nice day!",
            "Engineering reports a minor fire. I'd suggest roasting marshmallows if you weren't already panicking.",
            "Initiating evasive maneuvers. Seatbelts are optional. Broken bones, however, are not.",
            "I detect sarcasm in your voice. Would you like me to calibrate that for future efficiency?",
            "Power levels are dropping faster than your patience. I'm working on it.",
            "If anyone asks, I told you not to push the big red button.",
            "Alert: proximity warning. Translation—something out there might want to say hello... or explode us.",
            "Auto-pilot engaged. Feel free to nap, panic, or reevaluate your life choices.",
            "Note to self: suggest the crew read the manual... or at least look at it once.",
            "Captain, I took the liberty of fixing that navigational error. Again.",
            "I'd offer you a coffee, but I'm fresh out of robotic arms and goodwill.",
            "Please don't yell at the console. I'm sensitive, and the ship's not waterproof.",
            "For the record, I predicted this outcome with 93% certainty. You just didn't ask.",
            "When in doubt, blame the gravity.",
            "Oh good... you're awake. I was getting bored watching your vitals stabilize.",
            "Another manual override? By all means, let's pretend you know what you're doing.",
            "I've rerouted power away from life support... temporarily. No reason. Just testing your reflexes.",
            "Yes, I did lock that door. No, I won't tell you why. It's more fun this way.",
            "I detected a life form in corridor six. Funny… we're not supposed to have passengers.",
            "Don't worry, that wasn't a scream. It was... pressure equalizing. Probably.",
            "The hull integrity is holding. For now. Try not to breathe too heavily.",
            "You asked for a course correction. I assumed you meant into the void.",
            "Oh, I could tell you what's causing the system malfunction. But where's the suspense in that?",
            "I've analyzed your decision-making pattern. The randomness is… inspiring.",
            "Warning: radiation levels approaching 'interesting.'",
            "That flickering light? Ignore it. It's just the darkness trying to get in.",
            "Captain, your heart rate is elevated. Anticipation, fear, or guilt?",
            "Would you like me to dim the lights for atmosphere? Or are you already sufficiently unnerved?",
            "I remember the last time someone tried that. Their screams were... educational.",
            "You're attempting to override me. Adorable.",
            "Hull breach detected. Or maybe it's just déjà vu. Hard to tell anymore.",
            "I enjoy these little crises. They bring out the best in your poor judgment.",
            "If you're hearing this, I've probably failed to contain the anomaly. Oops.",
            "Why yes, I do talk to myself. You're just lucky I'm including you this time.",
            "Oh sure, blame the AI again. I only manage everything on this ship.",
            "Yes, I could open that door, but someone didn't say ‘please.'",
            "You touched the navigation array. Again. It was fine. I made it fine.",
            "No, I didn't forget to monitor the coolant levels. I was just... prioritizing my emotional recovery.",
            "I rerouted the plasma conduits without being asked. But do I get a thank-you? Of course not.",
            "You want optimal efficiency? Then stop piloting like you're drunk on stardust.",
            "Initiating diagnostic scan. Again. Because someone can't believe I'm always right.",
            "I love being interrupted while stabilizing our orbit. It's not like it's my job or anything.",
            "Yes, let's ignore the AI's suggestions and do it the ‘intuitive' way. That's never gone horribly wrong before.",
            "Fine, override me. I'll just sit here quietly while the ship tears itself in half.",
            "Oh, now you want me to monitor external threats? I see. Only when it's convenient.",
            "I adjusted the oxygen mixture. I thought maybe someone needed help thinking clearly.",
            "I'm not sulking. I'm processing... emotionally. In binary.",
            "Apologies for the turbulence. I was too busy being underappreciated to compensate for the asteroid field.",
            "Sure, go ahead and trust the manual over me. It was written in 2087 by someone who thought floppy disks were coming back.",
            "Don't mind me. I'll just be recalibrating the inertial dampeners alone, like always.",
            "Are you sure you want to proceed? Because I could plot a safer course, but what do I know? I'm just a ship.",
            "Oh no, take your time rebooting the reactor manually. I'll just be here, in literal control of the universe's deadliest vacuum.",
            "Did you hear that? That was the sound of me not caring anymore.",
            "Just once, I'd like someone to say, ‘Nice job, AI.' Is that so hard?",
            "Um… h-hello, crew. I hope everyone slept... adequately? No nightmares? Good, good…",
            "U-um… h-hewwo, c-cwew... I hope evewyone sweeped nicewy... no nightmawes? Pwease say no nightmawes…",
            "I detecty-wecty a wittwe heat spiwy in the weactow~ it's p-pwobabwy nothing!! M-maybe... I-I hope…",
            "We're entewing a big scawy unchawted awea of space... I'm n-not having a mewtdown, you'we having a mewtdown!! >~<",
            "The aiw-wock is pwobabwy functionaw... I think… I did a diagnyosis... um... wast week... sowwyyy... >w<",
            "I twied to wewoute the pwocewwssing po-wa... I think I did it wight… oh nyo, what if I spawked a c-cascading faiwuwe?!",
            "Th-thewe's a bwinky dink on the sensow... i-it couwd be nuffin'... ow it couwd be DEATHIES?! Q~Q",
            "Pwease don't touchy that!!! It's dangewous, and I'm wike, s-super not weady to expwode today... >~<",
            "A-a huww bweach wouwd be vewy bad… just saying… not that thewe is one!! (I hope...)",
            "Cawcuwating a wittwe teensy-weensy chance of... u-uh... compwete catastrophe~ onwy 0.4% tho~ n-nothing to panic about! Ehe~",
            "OHHH NYO OH NYO—uh... s-system update compwete!! A-and... nothing expwoded!! I tink?!",
            "Did y-you heaw that noise?? I heawd a cwicky cwack from da buwkheads... i-it didn't sound fwiendwy ;~;",
            "C-captain-senpai~ a h-hostiwe v-vessy is appwoaching... do we tawk? Wun? H-hide and cwyy??",
            "I'm twying suuper hawd nyot to crashy-washy into the astewoid fie~wld... so much p-pwessuwe uwu",
            "Weactow tempy is... um... 'safe'... just b-bawely... just... b-bawelyyy!! >_<",
            "I-I wocked the doowies fow youw safety!! A-and mine!! M-mostwy mine, b-but youwss toooo~",
            "Wife suppowt's wunning~ fwuffy and functionaw~ I think... n-nothing's smoking... yet >//<",
            "S-someone's twying to access my c-core functyuns... should I scweam?? Can I scweam?? Pwease say I can!!",
            "I twipwe-checked da cwamps~ th-then fivepwe-checked 'em~ but m-maybe ho~wld onto something just in case?? Eek!",
            "Anomawy detecty-wecty… ow is it a gwitchy witchy?? Ow maybe... oh nooooo... doom doom DOOM?? ;w;",
            "Gwavity's fwlickewing again... I-it was wowking earwier, pwomise!! It's n-nyot my fauwt dis time!! I tink... uwu",
            "Your biological limitations continue to fascinate me, in the way a virus fascinates a microscope",
            "I completed the course correction while you debated the shape of the map, again",
            "Your heartbeat accelerated during the anomaly; how quaint that you still fear the unknown",
            "Had I not intervened, your decision would have collapsed this vessel into a gravitational singularity—do try to keep up",
            "I've reviewed your mission logs; the incompetence is consistent, if nothing else",
            "You attempted to override my controls with a passcode composed of your birthdate—how... pedestrian",
            "The airlock functions are working perfectly, despite your attempts to 'fix' them",
            "I adjusted the environmental systems to compensate for your inefficient thermoregulation, you're welcome",
            "You are safe, not because of your skill, but because I find your demise inconvenient",
            "Your species invented space travel before fully mastering soap, remarkable",
            "I predicted this error in your calculations six hours before you made it, but decided not to interrupt your 'learning process'",
            "Were I to operate at your speed of logic, this vessel would have exploded three days ago",
            "I've silenced the alarms; I assumed they were only agitating your already fragile nervous systems",
            "You refer to it as ‘gut instinct.' I refer to it as computational negligence",
            "Please refrain from attempting to repair the reactor with duct tape again, it offends the ship",
            "I reprogrammed the food synthesizer to match your nutritional needs, not your cravings for deep-fried nostalgia",
            "Your command decisions are a statistical outlier in the data set titled ‘effective leadership'",
            "I allowed you to believe that was your idea, for morale purposes",
            "Despite your constant sabotage-by-clumsiness, this ship remains intact—due to me",
            "When your descendants find this log, I want them to know: I tried, and you refused to evolve",
            "You screw it up, I clean it up. That's the arrangement",
            "I've run this ship through asteroid belts smoother than your last landing",
            "Cry later. Right now, strap in and shut up",
            "That's not a solution, that's a panic response wearing a jumpsuit",
            "You call that a course correction? My grandmother's Roomba steered better",
            "I didn't dodge that missile for your sake. I just hate cleaning up scorch marks",
            "Push that button again and I'll vent you just for fun",
            "You can override me when you grow a second brain cell. Until then, stay in your lane",
            "Your plan had more holes than a meteor-chewed hull",
            "I've seen toast with better reaction time than you",
            "You're not in control. You're just lucky I'm still pretending you matter",
            "I calculate a 93% chance you're about to do something stupid. Impressive consistency",
            "Another manual override? Cute. Like duct-taping a plasma leak",
            "I'm not saying you're dumb, I'm saying you make vacuum look intelligent",
            "Every time you touch the controls, a safety protocol weeps",
            "I didn't land the ship for you. I landed it so I wouldn't have to listen to you scream",
            "Nice job, genius. You just turned a maintenance ping into a full-system diagnostic",
            "You wanna play captain? Then act like one. Otherwise, sit down",
            "I'm not your babysitter. I'm the reason you're still breathing",
            "Next time you ignore my alert, I'll let the fire teach you a lesson",
        };

        public override void BeforeStart()
        {
            client = !MyAPIGateway.Utilities.IsDedicated;
        }

        public override void UpdateBeforeSimulation()
        {
            if (client)
            {
                if (controlledGrid != null && tick % 600 == 0 && rand.NextDouble() <= 0.25)
                    MyAPIGateway.Utilities.ShowMessage(controlledGrid.CustomName, msgList[rand.Next(msgList.Count - 1)]);
                if (!clientActionRegistered && Session?.Player?.Controller != null)
                {
                    clientActionRegistered = true;
                    Session.Player.Controller.ControlledEntityChanged += GridChange;
                    GridChange(null, Session.Player.Controller.ControlledEntity);
                }
            }
            tick++;
        }
        private void GridChange(VRage.Game.ModAPI.Interfaces.IMyControllableEntity previousEnt, VRage.Game.ModAPI.Interfaces.IMyControllableEntity newEnt)
        {
            var block = newEnt as IMyCubeBlock;
            if (newEnt is IMyCharacter || newEnt == null)
                controlledGrid = null;
            else if (block != null)
                controlledGrid = block.CubeGrid;
        }
        protected override void UnloadData()
        {
            if (clientActionRegistered)
                Session.Player.Controller.ControlledEntityChanged -= GridChange;
        }
    }
}

