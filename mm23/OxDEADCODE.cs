using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OxDEADCODE : MonoBehaviour {
    public static OxDEADCODE AddYourselfTo(GameObject host) {
        return host.AddComponent<OxDEADCODE>();
    }
    
    [SerializeField]
    public CharacterScript character1;
    [SerializeField]
    public CharacterScript character2;
    [SerializeField]
    public CharacterScript character3;

    // block o' variable soup (a complete mess)
    private GameObject prefab1;
    private GameObject prefab2;
    private GameObject prefab3;
    private ObjectiveScript middleObjective;
    private ObjectiveScript leftObjective;
    private ObjectiveScript rightObjective;
    private ObjectiveScript closeObjective;
    private ObjectiveScript farObjective;
    private int timer = 0;
    private team ourTeamColor;
    private zone ourTeamZone;
    private bool reload1 = false;
    private bool reload2 = false;
    private bool reload3 = false;
    private float rlTime1 = 0.0f;
    private float rlTime2 = 0.0f;
    private float rlTime3 = 0.0f;
    private int hp1 = 100;
    private int hp2 = 100;
    private int hp3 = 100;
    private float htTime1 = -1.0f;
    private float htTime3 = -1.0f;
    private float htTime2 = -1.0f;

    void Start() {
        // get game specific objects
        character1 = transform.Find("Character1").gameObject.GetComponent<CharacterScript>();
        character2 = transform.Find("Character2").gameObject.GetComponent<CharacterScript>();
        character3 = transform.Find("Character3").gameObject.GetComponent<CharacterScript>();
        prefab1 = character1.getPrefabObject();
        prefab2 = character2.getPrefabObject();
        prefab3 = character3.getPrefabObject();

        middleObjective = GameObject.Find("MiddleObjective").GetComponent<ObjectiveScript>();
        leftObjective = GameObject.Find("LeftObjective").GetComponent<ObjectiveScript>();
        rightObjective = GameObject.Find("RightObjective").GetComponent<ObjectiveScript>();

        // determine color and zone
        ourTeamColor = character1.getTeam();
        if (ourTeamColor == team.red) {
            ourTeamZone = zone.RedBase;
            closeObjective = leftObjective;
            farObjective = rightObjective;
        } else {
            ourTeamZone = zone.BlueBase;
            closeObjective = rightObjective;
            farObjective = leftObjective;
        }

        // set priority
        // I choose LOWHP since it facilitates more 2 on 1 scenario
        // and a faster 5 points for the first kill
        character1.priority = firePriority.LOWHP;
        character2.priority = firePriority.LOWHP;
        character3.priority = firePriority.LOWHP;
    }

    void Update() {
        // I used all snipers for my startegy
        // the range provides safety

        // character 1 protrols the middle
        // character 2 protrols the right side
        // character 3 protrols the left side
        // (This is the same order they start in)
        if(character1.getZone() == ourTeamZone)
            character1.setLoadout(loadout.LONG);
        if(character2.getZone() == ourTeamZone)
            character2.setLoadout(loadout.LONG);
        if(character2.getZone() == ourTeamZone)
            character3.setLoadout(loadout.LONG);
        
        // target is the target location to move to
        // dir is the desired direction (when I'm not spinning)
        // more initialization of variables
        Vector3 target1 = middleObjective.transform.position;
        Vector3 target2 = rightObjective.transform.position;
        Vector3 target3 = leftObjective.transform.position;
        Vector3 dir1 = middleObjective.transform.position;
        Vector3 dir2 = rightObjective.transform.position;
        Vector3 dir3 = leftObjective.transform.position;
        bool enemy1 = false;
        bool enemy2 = false;
        bool enemy3 = false;
        float invRot = 1.0f;

        // the coordinates for where each character camps and ducks for cover
        // also where I exterminate the untrustworthy hiders in the top-left and bottom-right corners
        Vector3 camp2 = new Vector3(54.0f, 0, -1.0f);
        Vector3 camp3 = new Vector3(-22.9f, 0, -36.8f);
        Vector3 camp1 = new Vector3(27.61f, 0, -12.23f);
        Vector3 duck2 = new Vector3(51.53f, 0, -2.81f);
        Vector3 duck3 = new Vector3(-21.9f, 0, -34.87f);
        Vector3 duck1 = new Vector3(25.36f, 0, -14.12f);
        Vector3 exterm = new Vector3(-37.2f, 0, -20.3f);
        Vector3 ducke = new Vector3(-37.2f, 0, -15.2f);
        // these need to be flipped when are red (I based everything off the blue side)
        if (ourTeamColor == team.red) {
            Vector3 temp = camp2;
            camp2 = -camp3;
            camp3 = -temp;
            camp1 = -camp1;

            temp = duck2;
            duck2 = -duck3;
            duck3 = -temp;
            duck1 = -duck1;
            invRot = -1.0f;
        }

        // Code to change positions if it is desirable to do so
        // In order to do this I simply swap the character and prefab objects around

        // This condition says if chr1 (character 1) is close to its camping stop AND we own the middle objective AND we have at least 40 HP AND we don't see any enemies, 
        // then chr1 is available to take someones slot
        if(Vector3.Distance(prefab1.transform.position, camp1) < 2.0f && middleObjective.getControllingTeam() == ourTeamColor && character1.getHP() > 40 && character1.visibleEnemyLocations.Count == 0)
        {
            // if right is owned by the other team AND chr2 is dead, then we will switch
            if(rightObjective.getControllingTeam() != ourTeamColor && character2.getHP() == 0){
                CharacterScript temp = character2;
                character2 = character1;
                character1 = temp;
                GameObject temp2 = prefab2;
                prefab2 = prefab1;
                prefab1 = temp2;
            // same thing here with chr3
            } else if(leftObjective.getControllingTeam() != ourTeamColor && character3.getHP() == 0) {
                CharacterScript temp = character3;
                character3 = character1;
                character1 = temp;
                GameObject temp2 = prefab3;
                prefab3 = prefab1;
                prefab1 = temp2;
            }
        // test if the middle position is available to fill (see above for conditions)
        }else if(middleObjective.getControllingTeam() != ourTeamColor && character1.getHP() == 0) {
            // if it is able to be filled by chr2
            if(Vector3.Distance(prefab2.transform.position, camp2) < 2.0f && rightObjective.getControllingTeam() == ourTeamColor && character2.getHP() > 40 && character2.visibleEnemyLocations.Count == 0) {
                CharacterScript temp = character2;
                character2 = character1;
                character1 = temp;
                GameObject temp2 = prefab2;
                prefab2 = prefab1;
                prefab1 = temp2;
            // or maybe by chr3
            } else if(Vector3.Distance(prefab3.transform.position, camp3) < 2.0f && leftObjective.getControllingTeam() == ourTeamColor && character3.getHP() > 40 && character3.visibleEnemyLocations.Count == 0) {
                CharacterScript temp = character3;
                character3 = character1;
                character1 = temp;
                GameObject temp2 = prefab3;
                prefab3 = prefab1;
                prefab1 = temp2;
            }
        }

        // find the distances of each chr to its objective
        // (this no longer stands for min distance, it is a vestige of old code)
        float minDist1 = Vector3.Distance(target1, prefab1.transform.position);
        float minDist2 = Vector3.Distance(target2, prefab2.transform.position);
        float minDist3 = Vector3.Distance(target3, prefab3.transform.position);

        // target the camping spot is we own our objective or we are far away
        // (so we can use our range to our benefit)
        if (rightObjective.getControllingTeam() == ourTeamColor || minDist2 > 35.0f) {
            target2 = camp2;
        }
        if (leftObjective.getControllingTeam() == ourTeamColor || minDist3 > 35.0f) {
            target3 = camp3;
        }
        if (middleObjective.getControllingTeam() == ourTeamColor || minDist1 > 35.0f) {
            target1 = camp1;
        }

        // Late added hack to look at the corner if we are entering a side objective to catch those dirty hiders
        // Logic is: if we are between 3 and 8 units from the center of the objective look at the corner
        if(Vector3.Distance(rightObjective.transform.position, prefab2.transform.position) >= 3.0f && Vector3.Distance(rightObjective.transform.position, prefab2.transform.position) < 8.0f)
            dir2 = new Vector3(58.8f, 0, 35.7f);
        if(Vector3.Distance(leftObjective.transform.position, prefab3.transform.position) >= 3.0f && Vector3.Distance(leftObjective.transform.position, prefab3.transform.position) < 8.0f)
            dir3 = new Vector3(-58.8f, 0, -35.7f);

        // Search if there is any good candy, I mean items, on the ground nearby
        List<GameObject> items = character1.getItemList();
        foreach (GameObject item in items) {
            Vector3 itemPos = item.transform.position;
            typeOfItem itemType = item.transform.GetChild(2).GetComponent<ItemScript>().getTypeOfItem();
            float maxDist = 20.0f; // only go up to 20 units away to get candy
            if(itemType == typeOfItem.points)
                maxDist = 40.0f; // except for the extra tasty points candy that we will go 40 units for

            // for each chr look to see if it is close AND if its health we actually need the health AND we are not trying to cap an objective AND we are alive
            // we only want one chr at a time looking for candy (also the chr order is a bit weird. This is another vestige of old code)
            if (Vector3.Distance(prefab2.transform.position, itemPos) < maxDist && (itemType != typeOfItem.health || character1.getHP() < 60) && minDist2 > 5.0f && character2.getHP() > 0) {
                target2 = itemPos;
                break;
            } else if (Vector3.Distance(prefab3.transform.position, itemPos) < maxDist && (itemType != typeOfItem.health || character2.getHP() < 60) && minDist3 > 5.0f && character3.getHP() > 0) {
                target3 = itemPos;
                break;
            } else if (Vector3.Distance(prefab1.transform.position, itemPos) < maxDist && (itemType != typeOfItem.health || character3.getHP() < 60) && minDist1 > 5.0f && character1.getHP() > 0) {
                target1 = itemPos;
                break;
            }
        }

        // look to see if any chr has been hit
        int curHP1 = character1.getHP();
        int curHP2 = character2.getHP();
        int curHP3 = character3.getHP();
        // if (our current HP is less than last frame's OR its only been a second since we were last hit) AND we have data in the attackedFromLocations list AND we are alive
        if((curHP1 < hp1 || Time.time-htTime1 < 1.0f) && character1.attackedFromLocations.Count > 0 && character1.getHP() > 0) {
            // if we just got hit start the timer
            if (curHP1 < hp1)
                htTime1 = Time.time;
            // now follow the enemy as long as we are not capping an objective, and look at the enemy
            Vector3 enm = character1.attackedFromLocations[0];
            if (minDist1 > 5.0f)
                target1 = enm;
            enemy1 = true;
            dir1 = enm;
        }
        // same for chr2 and chr3
        if ((curHP2 < hp2 || Time.time - htTime2 < 1.0f) && character2.attackedFromLocations.Count > 0 && character2.getHP() > 0) {
            if (curHP2 < hp2)
                htTime2 = Time.time;
            Vector3 enm = character2.attackedFromLocations[0];
            if (minDist2 > 5.0f)
                target2 = enm;
            // except for this added statement that detects if some one is hiding in the corner
            // if so go to ther extermination stop (this codes isn't in chr1 since chr1 is in the center)
            if (isHiding(enm))
                target2 = -exterm;
            enemy2 = true;
            dir2 = enm;
        }
        if ((curHP3 < hp3 || Time.time - htTime3 < 1.0f) && character3.attackedFromLocations.Count>0 && character3.getHP() > 0) {
            if (curHP3 < hp3)
                htTime3 = Time.time;
            Vector3 enm = character3.attackedFromLocations[0];
            if (minDist3 > 5.0f)
                target3 = enm;
            if (isHiding(enm))
                target3 = exterm;
            enemy3 = true;
            dir3 = enm;
        }
        // store current frame's HP values for next frame
        hp1 = curHP1;
        hp2 = curHP2;
        hp3 = curHP3;

        // do the same thing with the enemies that we can see
        // (again weird vestigial order)
        if(character2.visibleEnemyLocations.Count > 0 && character2.getHP() > 0) {
            Vector3 enm = character2.visibleEnemyLocations[0];
            if (minDist2 > 5.0f)
                target2 = camp2;
            if (isHiding(enm))
                target2 = -exterm;
            enemy2 = true;
            dir2 = enm;
        }
        if (character3.visibleEnemyLocations.Count > 0 && character3.getHP() > 0) {
            Vector3 enm = character3.visibleEnemyLocations[0];
            if (minDist3 > 5.0f)
                target3 = camp3;
            if (isHiding(enm))
                target3 = exterm;
            enemy3 = true;
            dir3 = enm;
        }
        if (character1.visibleEnemyLocations.Count > 0 && character1.getHP() > 0) {
            Vector3 enm = character1.visibleEnemyLocations[0];
            if (minDist1 > 5.0f)
                target1 = camp1;
            enemy1 = true;
            dir1 = enm;
        }

        // This code makes the chr duck for cover to reload
        // if we are reloading AND haven't started the reload process yet AND we are close to our camping spot (and therefore our cover),
        if (character1.getReloadTime() > 0 && !reload1 && Vector3.Distance(prefab1.transform.position, camp1) < 2.0f){
            // then start the reload process and start a timer
            reload1 = true;
            rlTime1 = Time.time;
        // if we are still reloading AND going through the reload process (redundant) AND it hasn't been a second
        // (sometimes our gun fires straight away so it looks like we are still reloading, the 1 second timer guards for this),
        } else if (character1.getReloadTime() > 0 && reload1 && Time.time - rlTime1 < 1.0f) {
            // if its been less then 0.5 seconds go to the duck spot otherwise start heading back
            if (Time.time - rlTime1 < 0.5f)
                target1 = duck1;
        } else {
            // if we are done reloading (through 1 second passing of the getReloadTime function returning 0) end the reload prcess
            reload1 = false;
        }
        // with chr2 and chr3 we also have to duck and cover if we are at the extermination spot
        if (character2.getReloadTime() > 0 && !reload2 && (Vector3.Distance(prefab2.transform.position, camp2) < 2.0f || Vector3.Distance(prefab2.transform.position, -exterm) < 2.0f)) {
            reload2 = true;
            rlTime2 = Time.time;
        } else if (character2.getReloadTime() > 0 && reload2 && Time.time - rlTime2 < 1.0f) {
            if(Vector3.Distance(prefab2.transform.position, camp2) < Vector3.Distance(prefab2.transform.position, -exterm)) {
                if (Time.time - rlTime2 < 0.5f)
                    target2 = duck2;
            } else {
                if (Time.time - rlTime2 < 0.5f)
                    target2 = -ducke;
            }
        } else {
            reload2 = false;
        }
        if (character3.getReloadTime() > 0 && !reload3 && (Vector3.Distance(prefab3.transform.position, camp3) < 2.0f || Vector3.Distance(prefab3.transform.position, exterm) < 2.0f)) {
            reload3 = true;
            rlTime3 = Time.time;
        } else if (character3.getReloadTime() > 0 && reload3 && Time.time - rlTime3 < 1.0f) {
            if (Vector3.Distance(prefab3.transform.position, camp3) < Vector3.Distance(prefab3.transform.position, exterm)) {
                if (Time.time - rlTime3 < 0.6f) // I found that we can hang out here for 0.1 seconds longer without wasting time
                    target3 = duck3;
            } else {
                if (Time.time - rlTime3 < 0.5f)
                    target3 = ducke;
            }
        } else {
            reload3 = false;
        }

        // now that the code has argued over where are target is, finally set it in stone
        character1.MoveChar(target1);
        character2.MoveChar(target2);
        character3.MoveChar(target3);

        // rotation unfortunately is a little more complicated
        // first if we are really far away from the objective or (we are capping it AND not chasing an enemy/piece of candy)
        if (minDist1 > 35.0f || (Vector3.Distance(middleObjective.transform.position, prefab1.transform.position) < 3.0f && !enemy1)) {
            // then rotate. The rotation function is weird the closer to 180 the faster we rotate however, 
            // if we set it too close to 180 sometimes the it jerks back and forth erratically
            character1.rotateAngle(150.0f);
        } else {
            // The following code does several weird looking things
            // Visiblity detection is accomplished by tracing rays (20 in this case) and seeing if they collide with another character. 
            // However at the end of our range these rays spread out. This allows the possiblity for a character to be position between the
            // rays without touching them. This effect is worsened due to the fact that there are an even number of rays.
            // Meaning there is no central ray, so if you approach another character head on they will automatically slip into the gap.
            // The goal of the following code is to oscilliate our desired angle to catch a character that would otherwise slip through the gap.

            // complicated thing to generate the angles [-36.5, -36.5, 43.5, 43.5] periodically every 4 frames
            // while they seem like large angle the rotation algorithm limits the speed (proportionately to how far we are rotating)
            // and since we are onlygoing 2 frames in each direction we actally don't rotate that far.
            // Additonally, the 3.5 degree offset puts one of the rays in the center.
            float ang = ((Time.frameCount / 2) % 2 -0.5f)*80.0f + 3.5f;
            // Sometimes the oscillation gets out of hand so we need to make a limit where if we start too far away we don't oscillate
            // So calculate our desired heading
            Vector3 heading = dir1 - prefab1.transform.position;
            // Then find the difference between that and the actual direction we are facing
            float angDiff = Vector3.Angle(new Vector3(heading.x,0, heading.z), prefab1.transform.forward);
            // if it is greater than 90 degrees just set the offset angle to a constant value
            if (angDiff > 90.0f)
                ang = 3.0f;
            // we use a helper function (described below) to rotate our desired direction and apply it to our chr
            dir1 = rotate(dir1 - prefab1.transform.position, ang) + dir1;
            character1.SetFacing(dir1);
        }
        // repeat for chr2 and chr3
        if (minDist2 > 35.0f || (Vector3.Distance(rightObjective.transform.position, prefab2.transform.position) < 3.0f && !enemy2)) {
            // this makes the outside chrs rotate along the edge. This makes it easier to find those sneaky hiders
            character2.rotateAngle(-invRot*150.0f);
        } else {
            float ang = ((Time.frameCount / 2) % 2 - 0.5f) * 80.0f + 3.5f;
            Vector3 heading = dir2 - prefab2.transform.position;
            float angDiff = Vector3.Angle(new Vector3(heading.x, 0, heading.z), prefab2.transform.forward);
            if (angDiff > 90.0f)
                ang = 3.0f;
            dir2 = rotate(dir2 - prefab2.transform.position, ang) + dir2;
            character2.SetFacing(dir2);
        }
        if (minDist3 > 35.0f || (Vector3.Distance(leftObjective.transform.position, prefab3.transform.position) < 3.0f && !enemy3)) {
            character3.rotateAngle(invRot * 150.0f);
        } else {
            float ang = ((Time.frameCount / 2) % 2 - 0.5f) * 80.0f + 3.5f;
            Vector3 heading = dir3 - prefab3.transform.position;
            float angDiff = Vector3.Angle(new Vector3(heading.x, 0, heading.z), prefab3.transform.forward);
            if (angDiff > 90.0f){
                ang = 3.0f;
            }
            dir3 = rotate(dir3 - prefab3.transform.position, ang) + dir3;
            character3.SetFacing(dir3);
        }
    }

    // helper function that returns whether an enemies coordinates indicate that it is hiding in a corner
    public bool isHiding(Vector3 enemy){
        return (enemy.x > 44 && enemy.z > 21) || (enemy.x < -44 && enemy.z < -21);
    }

    // helper function to rotate a vector along the y-axis by a given angle
    public Vector3 rotate(Vector3 vect, float ang) {
        return Quaternion.Euler(0, ang, 0) * vect;
    }
}
