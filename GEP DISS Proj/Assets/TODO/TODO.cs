/*

TEXTURES:



    3. Setup Mountain

    4. Setup 'Collisions' (blank/transparent brush with collision boxes to prevent entities walking into ocean + mountains etc)     - DONE

    5. Animal assets
        a. At minimum need a single asset to represent any/all animals (can click on it/make colour changes to show different types - IE carnie/Herbie)
        b. Ideally a Herbivore (bunny) asset and a carnivores (bear/Wolf?) asset
        c. Icon or sprite??? unsure


CODING:

    1.  Human
        a. Need to walk around randomly
        b. Feel hunger, thirst and various other 'core' needs
        c. Basic GEP implemention (Genetic Algorithm to begin with)
            - 23 chromosomes (23rd determines gender only - however can include diseases)
            x) Will need to determine what will be stored genetically and what will be the goal etc (IE reach sentience for this project to more easily demo GEP)
                --- Hair,           (3) DONE - STILL NEED TO IMPLEMENT GENETIC HAIR COLOUR RATHER THAN PRE_SET
                --- Male/Female     - Special generation for this one. 
                                    - Initial symbol must be an operator (+-#~ where ~ is square - gives a 50-50 +- )
                                    - Afterwhich process continues as normal - if # or ~ came up, then it is Male (only 1 X chromosome) if + or - came up then Female (XX chromosomes)
                --- Skin Colour = RGB values stored on chromosomes        (3)   DONE - STILL NEED TO IMPLEMENT GENETIC HAIR COLOUR RATHER THAN PRE_SET
                --- Eye Colour = indices of set eye colours stored on chromosomes       (3 x 2) (RGB per eye)   DONE - STILL NEED TO IMPLEMENT GENETIC HAIR COLOUR RATHER THAN PRE_SET
                --- Need for food       (2)
                ---     water (how often food/water needs to be consumed - NOT related to intellect - this is like a 'base' level) (2)
   (DONE)       --- How quickly an individual grows (from baby -> adult)        (4)  
                --- Energy consumption (how quickly they get tired)     (2)
                --- Height / weight (init h/w + max Height?)        (2)
                --- Tendancy to gain weight         (2)
                --- Offspring number            (3)     (Affinity, not hard set/minimums, possibly maximum?. So high number may mean more likely for twins etc)
   (DONE)       --- pregnacy period         (2)
   (DONE)       --- Reproduction age        (2)        (Early reprodution = late life not important, but disastrous if die early --- Continous = early death disastrous to repro. fitness but long life = more repro)
   (DONE)       --- Life span (ignoring disease, and other factors)     (4)
                --- Sexual orientation(?????)                       (2) - 1 gay 1 straight gene. Both can be on or off. If both on, sex does not matter and will mate with anyone.
                                                                        - If both are off will not mate (may bond/form attachments?)
                                                                        - If Straight gene is off but gay is on, will only have sex with same sex. And vice versa.
                --- Sexual Selection Traits - Such as Peacock feathers which are detrimental evolutionary (harder to hide/fly etc) but magnet to other peacocks. 
                                            - This already done in a way with 'Vanity' but rather than an attribute, perhaps have actual traits which affect attraction (Charisma) on it's own
                                                and they may or may not have additional side effects (IE peacock feathers may reduce Constitution as harder to fight with them)
                                            -- (However many traits there are)
                                            -- Weapon Size
                                                    ----- Already done via "Weapon Reach" - just a different algorithm to determine sexual attractiveness (++ to Charisma)
                                            -- Feathers (and size/colour) - 2
                                            -- Scales (Colour) - 2
                                            -- Other Decorative Features (and colour/size etc) - 2
                --- Vision - Nocturnal vs Day time (if day-night cycles implemented - bonuses/limitations on the opposite)  (2) - similar to Orientation, both can be on or off etc
                            -- Could evolve both
                            -- If night vision, have green circle behind eyes?
                            -- Eye size also? 
                --- Weapons - Claws, Horns, Teeth, Tongue, poison, darts, stingers, barbs, tail etc (can have multiple)        (However many weapons there are - 8?) - each on or off
                --- Weapons Damage - Damage modifier each weapon does (per tick)                                        (However many weapons there are - 8 + arms?)    
                    -- Since the genes for damage + reach wouldn't technically exist until the weapon exists, can randomly insert the gene onto a chromosome when acquired?
                --- Weapon Reach - Range of a weapon's effect                                                           (However many weapons there are - 8 + arms?)
                    -- Since the genes for reach wouldn't exist until the weapon exists, can randomly insert the gene onto a chromosome when acquired?                
                --- Psionics Range - Range of Telekinesis. Default is 0, and is difficult to get above 1.0 (range it becomes useful)       (2)
                --- reach - Determines range of interaction   - If 'Psionics' (or telekinsis) is evolved, it is multiplied by Psi-range)   (2)
                --- Olfactory sensitivity (How good is smell? - Bonus to Hunting prey - detection radius INC)       (2)
                --- Auditory Sensitivity (how good is hearing? - bonus to hunting prey - detection radius INC)      (2)
                --- Gestation type (Mammalian, reptile etc)     (1)
                                                              - eggs take say 1/3 time to pop out at which point the mother is free to return to whatever she was doing before. But egg needs
                                                              constant heat + protection until hatched
                --- Pheremones - Maintain detection presense for longer. Allows other humans to find an area/individual (Ant-Colony behaviour)         (2  see below)
                                - Can be used to attract/trap predators
                                - Prevent getting lost
                                - Find mate/attract mate (+ charisma rating?)
                                - Could be used to repel predators (with sensitive noses)?
                                ()()() --- if first is higher than second then is attractive (to both mates + Predators),
                                           --- if second is higher then is repellant (to just predators - doesn't affect mates and can still be used for navigation etc ) 
                --- Certain programmed diseases hardcoded onto chromosomes (genetic diseases - IE blond hair = positive for disease etc )
                    -- 1 Instantly kills the individual (on birth)              (2)
                    -- 1 Severely reduces one or more of the attributes at random    (4)
                    -- 1 Gradually affects one or more of the attributes at random over time - if any attribute     (6)
                    -- Possible disease is to set an extremely high value for Energy Consumption, causing individuals to be tired constantly (if energy hits 0, they 'pass out' from tiredness)
                        -- If pass out whilst starving or completely dehydrated, can cause damage/death etc     (2)
                --- 6 Attributes listed below       (18) (3 per take average)
                --- Detection Radius - Not only is it used in hunting, but it is also used in keeping track of where the population/home is (so if hunting and goes too far can get lost)
                        -- Not directly programmed in genes. Which radius depends on what is being detected.
                            -- Sight is everything and is usually the lowest due to visibility
                            -- Smell = Hunting for food (both live, dead + plant)  -- Also used for phermones
                            -- Sound = Predators + mates

            xx) 4 Survival Needs which determine which priority to use below (Per individual);
                --- Safety - How safe is a population from attack/extinction. Refers to predation, are the predators very dangerous, or are the population able to handle the predators
                             well enough so that they aren't a problem?
                           - High Str/Const can reduce the need to feel safe as are confident enough to defend themselves.
                --- Food   - What are the food supplies like here? Is there sufficient food for everyone and for how long. Is there low food sources in the area or high?
                           - Food is stored as a population-wide resource. When food is low, indiv. in pop'n will go out and hunt more food.
                           - High intl = can survive periods of low/no food levels longer.
                --- Water  - Is there water near by? Individuals must regularly visit FRESH water sources to prevent dying from thirst. 
                           - Water cannot be stored (initially - mid game it can as intelligence grows). Water sources are gauged on distance + safety of source. 
                           - Intelligence helps with reducing this need...high intl = need to visit fresh water less and less.
                --- Space  - How crowded is the current area? If density is too high, then go out and find a new suitable area.

            xxx) Have a two tier Evolutionary Priority system used to determine 'Fitness' of a POPULATION (these are population wide)
                --- Uses a scale of 0.0 - 1.0. If Survival is at 0.75, then Quality will be at 0.25 (meaning 25% pop will go based on Quality, and 75% look at Survival priorities)
                --- In a perfect environment it is still possible to achieve sentience, but at a much slower/random rate.
                --- Priority    A = "Survival" of individual(s); Includes attributes such as:  Strength (combat damage...Const + Str = HP), 
                                                                                                    Represents Minimum stre. Can Inc and Dec but never drop below init val
                                                                                                     (unless affected by disease #3)
                                                                                                     HP can change over time also once per "year"
                                                                                               Intellect (Adaption - Progress to Sentience too), 
                                                                                                    Represents maximum intellect. Cannot increase beyond but  can decrease.
                                                                                                    Starts at 0 at birth and gradually increases.
                                                                                                    Used in equation to determine whether can solve a problem (IE adaption problem)
                                                                                                        Bonus to Fighting
                                                                                                        Adaption also includes food shortages etc (higher int. = live longer when hungry)
                                                                                               Constitution (Physical fitness - Disease resist. + bonus in Combat/Hunting etc?)
                                                                                                    Const represents minimum fitness...can increase + dec. But never go below init val
                                                                                                     (unless affected by disease #3)
                                    
                                    B Is ultimately a 'Bogus' priority. If the environment is perfect, with no predators or competition, then there is no need for survival, as such
                                        No need to evolve specific traits - in QoL mode, 
                                    However, if Safety, Food + Water survival needs are all satisfactory in an "imperfect" environment, then B will take presedence anyway

                                B = "Quality" of Pop/Life; Could include attributes such as:   Wisdom (Writing/art - needed to progress beyond a certain intellect toward sentience?)
                                                                                               Vanity (Predisposition to prefering physically attractive mates (high constitution + Charisma) )
                                                                                                        --- Sexual Selection vs Evolutionary Selection
                                                                                               Charisma (Likelihood of being chosen as a mate in a perfect environment)
                                                                                                    Low Charisma in a 'perfect environment' means that individual is unlikely to reproduce
                                                                                                    even if they have extremely high intellect or strength.

                                In this system, if a lot of people are dying from disease or predation, then "Survival" will become a higher priority than quality of life.
                                The population (or individuals?) will then try to determine why the population is dying and how that individual believes is the best mate to survive.
                                    IE if a population is dying a lot from a disease, then the population will focus on survival. The individuals however do not know that const. is
                                        best used for survival, so will look at who is getting ill and surviving (or not getting ill best case) and try mate with them - likely because
                                        of high constitution
                                    IE if a population is dying because of predators attacking, then those who are strong will be the best mates. In this case strength is obvious as
                                        it is shown phenotypically (IE Muscular etc). However an indiv. with very high constution may still make a good fighter but not necessarily be
                                        strong. Likewise a smart fighter may also not be the strongest or most fit. As such the individuals will determine themselves who is best
                                            Could go based off who looks the strongest (highest strength stat), or base it off of how successful they are (Int + Const as these are not
                                            very distinguishable physically, whilst a const person may appear physically strong, may not actually be strong etc)
                                    IE if a population is dying because of a change in the climate, or local conditions, then the smart individuals who are best able to adapt may be the
                                        best mates. This can also be Constitution (resistance to change) through physical well-being, however only goes so far.
                                            Can be justified as something like the intellectual people think of ways to adapt (IE if a river constantly floods - move away). 
                                            In game, this example could be if a player places a new mountain, that cuts off a prime food source, the smart may move to a better location.
                                                THIS WILL NEED TO ALSO HAVE THE POPULATION CHECK WHERE IS SUITABLE TO LIVE

            IV) Notice how 'Reproduction' is not listed. This is because this will happen under 4 conditions;
                1> Individual(s) are of sufficient age 
                2> Individual(s) both have sufficient food + water needs met.
                3> Individual(s) both feel safe enough - does not have to be perfectly safe overall, but no predators may be near by (mood killer)
                4> Individual(s) both have enough energy levels. Energy is tiredness - when this is low the indiv. is less willing to do anything except sleep (including hunt, run etc)
                    Cannot directly die from exhaustion, but rather other effects, IE too tired to run from a fight
                    A potential disease could make an individual tired regardless, including too tired to eat, drink and prioritises sleeping a lot more than normal/necessary (akin to CFS)

    2. AI
        a. Need to include some animals (carnivores IE bears, and herbivores IE bunnies - can use a different image for Carnies/Herbies or a generic "animals" asset?)
        b. Herbivores will flee from everything but other herbivores, and are easy to hunt (in beginning)
            x) Could evolve them to combat environment ie; fast breeders, become more aggressive/pack animals, develop weapons (IE claws) making them harder to hunt
        c. Carnivores will chase herbivores + humans (See 'SPECIES: ALRE' Fear vs Anger stats for humans. If there are many humans, Carnie may flee + likewise with humans)
            x) Could evolve them as time progresses (slower than herbies because not as dire need) such as pack instinct. 
            xx) Could also include 'territorialism' where other carnies get chased off if male and in other territory (could lead to pack mentalities?)


 
 */