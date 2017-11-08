using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyWiFi.ServerBackchannels;
using UnityEngine.UI;
using EasyWiFi.Core;
using System;

public class MagiaTransformo : MonoBehaviour {
         

    // is
    public bool isTutorial = true;
    public bool isSerialRFID = true;
    public bool isSerialLED = true;
    public bool isWifi = true;
    public bool isVision = false;
    public bool isDepth = false;
    public bool isRedBook = true;
    public bool isYellowBook = true;
    public bool isGreenBook = true;
    public bool isWizard = false; // Wizard of OZ version
    public bool isPlayIntro = true;
    public bool isPlayStory = true;
    public bool isDev = false;
    public bool isDevA = true;
    bool isReset = false;


    public bool isHatOn = false;
    public bool isCloakOn = false;

    public bool isTrackingAll = false;
    public bool isArrivedAll = false;
    public string tutorialName = "The Hidden Orders"; // // "The Golden Societies" "The Hidden Orders"

    public float chantLineTime = 3.0f;
    public float loudnessThreshold = 200.0f;

    public void setLoudnessThreshold(float ld)
    {
        loudnessThreshold = ld;
    }
    
    // Controllers - wired connections
    public SerialController serialControllerRFID;
    public SerialController serialControllerLED;
    // public VisionController visionController;

    // Wifi channels
    public StringServerBackchannel msg1, nav1;
    public StringServerBackchannel msg2, nav2;
    public StringServerBackchannel msg3, nav3;
    public StringServerBackchannel gameState;

    // depth controller
    // public Depth depthController;

    // Values
    // loudness and gyros
    int ld1 = 0, ld2 = 0, ld3 = 0;

    string itemScanned;
    int costumeIndex = -1;
    int itemsSelected = 0;

    int characterNum = 0;
    string witch1;
    string witch2;
    string witch3;

    string RFID; // raw RFID

    // GUI displays
    string wifiMsg1 = "null", wifiMsg2 = "null", wifiMsg3 = "null";
    string navMsg1 = "null", navMsg2 = "null", navMsg3 = "null";
    string ledMsg = "null";
    string tracking1 = "null", tracking2 = "null", tracking3 = "null";
    string gameStateMsg;
    string spell; // spell is sent when gameState is the result

    public Text wifiMsgDisplay1, wifiMsgDisplay2, wifiMsgDisplay3;
    public Text navDisplay1, navDisplay2, navDisplay3;
    public Text ledMsgDisplay; //, spellTextDisplay, gameStateDisplay, 
    public Text trackingDiplay1, trackingDiplay2, trackingDiplay3;
    public Text loudnessText;
    public Text gyroText;
    public Text eulerText;
    
    // dance
    public bool isSpin1 = false;
    public bool isSpin2 = false;
    public bool isSpin3 = false;
    public bool isSpinAll = false;
    Quaternion q1, q2, q3;
    Vector3 euler1, euler2, euler3;
    bool[] orientations1 = { false, false, false, false }; // 4 quadrants
    bool[] orientations2 = { false, false, false, false }; // 4 quadrants
    bool[] orientations3 = { false, false, false, false }; // 4 quadrants
    
    public bool isRaising = false;

    int chantLine = 0;
    int chantMode = 1;

    string[] unison = { "Fire, from our cauldron rise", "Torch the air and light the skies", "Spirits from the other side",
        "Join us in our wild ride", "Dancing circles in the night", "Summon wisdom for our rite"};

    string[,] call_response =
        {
            {"By ancient Abrem-melin", "...", "..."},
            {"...", "We conjure power!", "We conjure power!"},
            {"...", "By slumbering Merlin", "..."},
            {"We conjure wisdom!", "...", "We conjure wisdom!"},
            {"...", "...", "By the studious Starhawk"},
            {"We conjure knowledge!", "We conjure knowledge!", " " },
            
        };
    static Dictionary<string, string> cauldronLookup = new Dictionary<string, string>() {
            {"Fire, from our cauldron rise", "R"}, {"Torch the air and light the skies", "L"}, {"Spirits from the other side", "Y"},
            {"Join us in our wild ride", "X"}, {"Dancing circles in the night", "O"}, {"Summon wisdom for our rite", "S"},
            {"By ancient Abrem-melin", "X"},        {"We conjure power!", "Y"},     {"By slumbering Merlin", "O"},
            { "We conjure wisdom!", "I"},        {"By the studious Starhawk", "s"},     { "We conjure knowledge!", "K"},
            {"Spin1", "j"}, {"Spin2", "k"}, {"Spin3", "l"}, {"Center", "c"}, {"The Golden Societies", "G"},
            {"The Hidden Orders", "H"}, {"Left" , "g"}, {"Right", "h"}, {"GoldenLightning", "Y"},
            {"HiddenLightning", "p"}, {"GoldenCircle", "o" }, {"HiddenCircle", "i" }, {"increaseStrikeTimes" , "b" },
            {"decreaseStrikeTimes", "n"},
        };
    // list of the costumes for each witch 
    //		stored in form [Hat1, Cloak1, Hat2, Cloak2, Hat3, Cloak3]
    string[] costumes;

    // look-ups
    // costume RFIDs -> corresponding strings
    //		energy items use "N-ergy" since witchLookup keys depend on first letter of costumeLookup values
    static Dictionary<string, string> costumeLookup = new Dictionary<string, string>() {
        {"535384", "Fire Hat"}, {" 0x4 0x24 0xFD 0x72 0xDF 0x4C 0x81", "Fire Hat"}, {" 0x64 0xEC 0x12 0x11", "Fire Hat"},
        { "4FCC6E4", "Fire Cloak"}, { " 0xE4 0xC6 0xFC 0x4", "Fire Cloak"}, {" 0x4 0xF3 0xFF 0x72 0xDF 0x4C 0x80", "Fire Cloak"},
        {"1112EE74", "Water Hat"}, {" 0x4 0xEB 0xFF 0x72 0xDF 0x4C 0x80", "Water Hat"}, {" 0xB4 0x20 0x3 0x5", "Water Hat"}, 
        { "4F36794", "Water Cloak"}, { "594294", "Water Cloak"}, {" 0x4 0xFB 0xFF 0x72 0xDF 0x4C 0x80", "Water Cloak"}, {" 0x74 0xEE 0x12 0x11", "Water Cloak"},
        {"116314", "Earth Hat"}, {" 0x4 0x1C 0xFD 0x72 0xDF 0x4C 0x81", "Earth Hat" }, {" 0xA4 0xF2 0x12 0x11", "Earth Hat"},
        { "4F0B144", "Earth Cloak"}, {" 0x4 0xD 0xFE 0x72 0xDF 0x4C 0x81", "Earth Cloak"}, {" 0x44 0xB1 0xF0 0x4", "Earth Cloak"},
        {"10FAA824", "Air Hat"}, {" 0xEE 0xAC 0x41 0x70", "Air Hat"}, {" 0x84 0x53 0x3 0x5", "Air Hat"}, {" 0x24 0xA8 0xFA 0x10", "Air Hat"},
        { "4F6AEC4", "Air Cloak"}, { " 0xC4 0xAE 0xF6 0x4", "Air Cloak"}, {" 0x90 0xAA 0x20 0x27", "Air Cloak"},
        {"1143A24", "Darkness Hat"}, {" 0x4 0x6 0xFF 0x72 0xDF 0x4C 0x81", "Darkness Hat"}, {" 0x4 0x9C 0xEB 0x4", "Darkness Hat"}, {" 0x94 0x9C 0xFA 0x10", "Darkness Hat"},
        { "10FA9C94", "Darkness Cloak"}, {" 0x4 0xE3 0xFF 0x72 0xDF 0x4C 0x80", "Darkness Cloak"}, {" 0x24 0x3A 0x4 0x11", "Darkness Cloak"},
        {"5349094", "N-ergy Hat"}, {" 0x4 0x15 0xFE 0x72 0xDF 0x4C 0x81", "N-ergy Hat"}, {" 0x94 0x67 0xF3 0x4", "N-ergy Hat"},
        { "10FBC844", "N-ergy Cloak"}, { " 0x44 0xC8 0xFB 0x10", "N-ergy Cloak"}, {" 0x4 0xDB 0xFF 0x72 0xDF 0x4C 0x80", "N-ergy Cloak"}
    };
    static Dictionary<string, string> witchLookup = new Dictionary<string, string>() {
        {"FF", "Fire"},         {"WW", "Water"},        {"EE", "Earth"},
        {"AA", "Air"},      {"DD", "Dark"},         {"NN", "Energy"},
        {"FW", "Steam"},        {"FE", "Lava"},     {"FA", "Fire Air"},     {"FD", "Dark Fire"},        {"FN", "Fire Energy"},
        {"WE", "Mud"},      {"WA", "Rain"},     {"WD", "Dark Water"},       {"WN", "Water Energy"},
        {"EA", "Dust"},     {"ED", "Dark Earth"},       {"EN", "Gem"},
        {"AD", "Tornado"},      {"AN", "Wind"},
        {"DN", "Dark Energy"}
    };
    // Use this for initialization

    void Start() {

        wifiAll("clear");
        chantLine = 0;
        isSpin1 = false;
        isSpin2 = false;
        isSpin3 = false;
        isSpinAll = false;

        if (isDevA)        
            StartCoroutine(AfterDressingUp());
        else
            StartCoroutine(Starting());
    }

    void Update()
    {

        if (isSerialRFID)
        {
            RFID = serialControllerRFID.ReadSerialMessage();

            if (RFID != null)
            {
                if (ReferenceEquals(RFID, SerialController.SERIAL_DEVICE_CONNECTED))
                    Debug.Log("Connection established RFID");
                else if (ReferenceEquals(RFID, SerialController.SERIAL_DEVICE_DISCONNECTED))
                    Debug.Log("Connection attempt failed or disconnection detected");
                else
                {
                    Debug.Log("Message arrived: " + RFID);

                    if (RFID.Substring(0, 2) != "FF") // sometimes a tag comes too fast and its ID becomes FFFFFF
                        OnCostumeScanned(RFID); // scanned costume tags
                }
            }

        }
        if (isSerialLED)
        {
            string message = serialControllerLED.ReadSerialMessage();
            if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_CONNECTED))
                Debug.Log("Connection established COM LED");
            else if (ReferenceEquals(message, SerialController.SERIAL_DEVICE_DISCONNECTED))
                Debug.Log("Connection attempt failed or disconnection detected");
        }
        
        DetectSpin();

        if (isReset)
        {
            StopAllCoroutines();
            wifiAll("reset");                      
            gameState.setValue("clear");
            isReset = false;
            Start();
        }

        keyboardControls();
        
    }

    
    IEnumerator Starting()
    {
        led("Z");

        AudioController.SetGlobalVolume(0.7f);
        AudioController.PlayMusic("intro");

        if (tutorialName == "The Hidden Orders")
        {
            AudioController.SetCategoryVolume("Music", 0.6f);
            AudioController.SetCategoryVolume("AlisterVoicesOneTime", 1.0f);
            AudioController.SetCategoryVolume("AlisterVoicesRepeated", 1.0f);                      

        }
        else
        {
            AudioController.SetCategoryVolume("Music", 0.6f);
            AudioController.SetCategoryVolume("AlisterVoicesOneTime", 0.90f);
            AudioController.SetCategoryVolume("AlisterVoicesRepeated", 0.90f);
        
        }
        if (isPlayIntro && !isDev)
        {
            // audio intros
            yield return new WaitForSeconds(3);
            AudioController.Play("Intro1");
            yield return new WaitForSeconds(26);
            //AudioController.Play("Evil Laugh 2");
            //yield return new WaitForSeconds(6);
            AudioController.Play("NewIntro2");
            yield return new WaitForSeconds(21);
            AudioController.Play("NewIntro3");
            yield return new WaitForSeconds(21);
        }        

        // tutorials
        if (isTutorial)
        {
            isCloakOn = false;
            isHatOn = false;

            costumes = new string[6];
            chantMode = UnityEngine.Random.Range(0, 2);

            characterNum = 1; // first player starts building character
            if (isDev)
            {

                yield return new WaitForSeconds(5f);
                wifi1("Water Cloak");
                wifi2("Earth Cloak");
                wifi3("Darkness Cloak");

                yield return new WaitForSeconds(1f);

                wifi1("Water Hat");
                wifi2("Earth Hat");
                wifi3("Darkness Hat");

                yield return new WaitForSeconds(1f);
                wifiAll(tutorialName); // spell

                wifiGameState("spell casting");
                StartCoroutine(ClearChannel(gameState));

                yield return new WaitForSeconds(1f);

                chantMode = UnityEngine.Random.Range(0, 2);

                yield return StartCoroutine(AfterDressingUp());

            }
            else
            {
                if (tutorialName == "The Golden Societies")
                {
                    yield return StartCoroutine(FirstInitiateGolden());
                }
                else
                {
                    yield return StartCoroutine(FirstInitiateHidden());
                }
            }


        }


    }
    IEnumerator FirstInitiateGolden()
    {
        characterNum = 1; // first player starts building character

        isCloakOn = false;
        costumes[1] = null;

        isHatOn = false;
        costumes[0] = null;

        //AudioController.Play("F");
        //yield return new WaitForSeconds(2);

        //led
        led("M"); // medium brightness
        yield return new WaitForSeconds(1);
        led("X"); // X: random cauldron

        AudioController.Play("Fire1");
        // yield return new WaitForSeconds(30);


        while (!isCloakOn)
        {

            // Fire2_success
            if (costumes[1] == "Fire Cloak")
            {
                wifi1("Fire Cloak");
                led("F");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play("F");

                yield return new WaitForSeconds(2);

                AudioController.Play("Fire2_success");
                yield return new WaitForSeconds(11);

                isCloakOn = true;
            }
            // Fire2_fail
            else if (costumes[1] != null)
            {
                wifi1("Null Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                costumes[1] = null;

                AudioController.Play(costumes[0].Substring(0, 1));

                yield return new WaitForSeconds(2);
                AudioController.Play("Fire2_fail");
            }
            else
            {
                if (!AudioController.IsPlaying("Fire2_wait") && !AudioController.IsPlaying("Fire2_fail") && !AudioController.IsPlaying("Fire1"))
                {
                    AudioController.Play("Fire2_wait");
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        led("f"); // small letter theater chase effect
        // Fire3
        AudioController.Play("Fire3");
        // yield return new WaitForSeconds(15);
        yield return new WaitForSeconds(1.0f);
        led("B"); // bright brightness

        // Hat
        while (!isHatOn)
        {
            
            // Fire4_success
            if (costumes[0] == "Fire Hat")
            {
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                wifi1("Fire Hat");
                AudioController.Play("F");
                led("F"); // bright brightness

                yield return new WaitForSeconds(2);
                AudioController.Play("Fire4_success");
                yield return new WaitForSeconds(11);

                isHatOn = true;
            }
            // Fire4_fail
            else if (costumes[0] != null)
            {
                wifi1("Null Hat");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[0].Substring(0, 1));
                costumes[0] = null;
                yield return new WaitForSeconds(2);

                AudioController.Play("Fire4_fail");
            }
            else
            {
                if (!AudioController.IsPlaying("Fire4_wait") && !AudioController.IsPlaying("Fire4_fail") && !AudioController.IsPlaying("Fire3"))
                {
                    AudioController.Play("Fire4_wait");
                }
            }

            // prevent crash
            yield return new WaitForSeconds(0.5f);
        }

        witch1 = "Fire Witch";

        if (isPlayStory)
        {
            // Mirror 1
            AudioController.StopCategory("AlisterVoicesRepeated", 1f);
            yield return new WaitForSeconds(1);
            AudioController.Play("Mirror1");
            yield return new WaitForSeconds(6);
            //AudioController.Play("Mirror2");
            //yield return new WaitForSeconds(6);
            //AudioController.Play("FireHat");
            //yield return new WaitForSeconds(23);
            //AudioController.Play("FireCloak");
            //yield return new WaitForSeconds(16);
            AudioController.Play("FireCoven");
            yield return new WaitForSeconds(13);

            // spellBook1

            AudioController.Play("NewSpellbook1");
            yield return new WaitForSeconds(12);

            // spellBook2
            //if ((isWizard == false) && (isVision) )
            //{
            //    while (!visionController.red_book.isTrackedm)
            //    {
            //        //spellBook2_wait
            //        AudioController.Play("Spellbook2_wait");
            //        yield return new WaitForSeconds(10);

            //    }

            //Spellbook2_first_success

            AudioController.Play("NewSpellbook2_first_success");
            yield return new WaitForSeconds(12);
            //}

        }





        yield return StartCoroutine(SecondInitiateGolden());

    }

    IEnumerator SecondInitiateGolden()
    {
        led("M"); // medium brightness
        yield return new WaitForSeconds(1);
        led("X"); // X: random cauldron

        characterNum = 2; // first player starts building character

        isCloakOn = false;
        costumes[3] = null;

        isHatOn = false;
        costumes[2] = null;

        //AudioController.Play("A");
        //yield return new WaitForSeconds(2);
        AudioController.StopCategory("AlisterVoicesRepeated", 1f);
        AudioController.StopCategory("AlisterVoicesOneTime", 1f);
        AudioController.Play("NewAir1");
        // yield return new WaitForSeconds(50);

        while (!isCloakOn)
        {
            // Air2_success
            if (costumes[3] == "Air Cloak")
            {
                led("A"); // medium brightness
                wifi2("Air Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play("A");

                yield return new WaitForSeconds(2);
                AudioController.Play("Air2_success");
                yield return new WaitForSeconds(10);

                isCloakOn = true;
            }
            // Air2_fail
            else if (costumes[3] != null)
            {
                wifi2("Null Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[3].Substring(0, 1));
                costumes[3] = null;
                yield return new WaitForSeconds(2);

                AudioController.Play("Air2_fail");
            }
            else
            {
                if (!AudioController.IsPlaying("Air2_wait") && !AudioController.IsPlaying("Air2_fail") && (!AudioController.IsPlaying("NewAir1")))
                {
                    AudioController.Play("Air2_wait");
                }
            }

            yield return new WaitForSeconds(0.5f);

        }

        led("a");
        yield return new WaitForSeconds(1);
        
        // Air3
        AudioController.Play("Air3");
        // yield return new WaitForSeconds(15);

        // Hat
        while (!isHatOn)
        {
            if (!AudioController.IsPlaying("Air4_wait") && !AudioController.IsPlaying("Air4_fail") && !AudioController.IsPlaying("Air3"))
            {
                AudioController.Play("Air4_wait");
            }
            // Air4_success
            if (costumes[2] == "Air Hat")
            {
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                wifi2("Air Hat");
                led("B");
                AudioController.Play(costumes[2].Substring(0, 1));

                yield return new WaitForSeconds(2);
                led("A");
                AudioController.Play("Air4_success");
                yield return new WaitForSeconds(9);

                isHatOn = true;
            }
            // Air4_fail
            else if (costumes[2] != null)
            {
                wifi2("Null Hat");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[2].Substring(0, 1));

                costumes[2] = null;
                yield return new WaitForSeconds(2);
                AudioController.Play("Air4_fail");
            }

            yield return new WaitForSeconds(0.5f);
        }

        witch2 = "Air Witch";

        // Mirror 1
        AudioController.StopCategory("AlisterVoicesRepeated", 1f);
        yield return new WaitForSeconds(1);

        if (isPlayStory)
        {
            AudioController.Play("Mirror1");
            yield return new WaitForSeconds(6);

            //AudioController.Play("AirHat");
            //yield return new WaitForSeconds(17);
            //AudioController.Play("AirCloak");
            //yield return new WaitForSeconds(21);
            AudioController.Play("AirCoven");
            yield return new WaitForSeconds(13);

            AudioController.Play("NewSpellbook1");
            yield return new WaitForSeconds(12);
        }



        // spellBook1
        // wifi2 spellcasting page
        // wifi2("spell casting");

        if (isPlayStory)
        {
            //Spellbook2_first_success
            //AudioController.StopCategory("AlisterVoicesRepeated", 1f);
            //yield return new WaitForSeconds(1);
            AudioController.Play("NewSpellbook2_second_success");
            yield return new WaitForSeconds(6); //12
        }
        yield return StartCoroutine(ThirdInitiateGolden());
        
    }

    IEnumerator ThirdInitiateGolden()
    {

        isCloakOn = false;
        costumes[5] = null;

        isHatOn = false;
        costumes[4] = null;

        led("M");
        yield return new WaitForSeconds(1);
        led("X");
        //AudioController.Play("N");
        //yield return new WaitForSeconds(2);

        // Energy1
        AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
        AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

        AudioController.Play("NewEnergy1");
        // yield return new WaitForSeconds(38);

        characterNum = 3; // third player starts building character

        while (!isCloakOn)
        {
            // Energy2_success
            if (costumes[5] == "N-ergy Cloak")
            {
                led("N");
                wifi3("N-ergy Cloak");

                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play("N");

                yield return new WaitForSeconds(2);
                AudioController.Play("Energy2_success");
                yield return new WaitForSeconds(10);

                isCloakOn = true;
            }
            // Energy2_fail
            else if (costumes[5] != null)
            {
                wifi3("Null Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[5].Substring(0, 1));
                costumes[5] = null;

                yield return new WaitForSeconds(2);
                AudioController.Play("Energy2_fail");

            }
            else
            {
                if (!AudioController.IsPlaying("Energy2_wait") && !AudioController.IsPlaying("Energy2_fail") && !AudioController.IsPlaying("NewEnergy1"))
                {
                    AudioController.Play("Energy2_wait");
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Energy3
        AudioController.Play("Energy3");
        // yield return new WaitForSeconds(14); // must be commented out to be interuptable
        led("B");
        // Hat
        while (!isHatOn)
        {

            // Energy4_success
            if (costumes[4] == "N-ergy Hat") //! Energy
            {
                wifi3("N-ergy Hat");
                led("N");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play("N");

                yield return new WaitForSeconds(2);

                AudioController.Play("Energy4_success");
                yield return new WaitForSeconds(10);

                isHatOn = true;
            }
            // Energy4_fail
            else if (costumes[4] != null)
            {
                wifi3("Null Hat");

                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[4].Substring(0, 1));
                costumes[4] = null;

                yield return new WaitForSeconds(2f);
                AudioController.Play("Energy4_fail");

            }
            else
            {
                if (!AudioController.IsPlaying("Energy4_wait") && !AudioController.IsPlaying("Energy4_fail") && !AudioController.IsPlaying("Energy3"))
                {
                    AudioController.Play("Energy4_wait");
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        witch3 = "N-ergy Witch";

        //EnergyHat
        if (isPlayStory)
        {
            AudioController.StopCategory("AlisterVoicesRepeated");
            AudioController.Play("Mirror1");
            yield return new WaitForSeconds(6);

            //AudioController.Play("EnergyHat");
            //yield return new WaitForSeconds(34);
            //AudioController.Play("EnergyCloak");
            //yield return new WaitForSeconds(21);
            AudioController.Play("EnergyCoven");
            yield return new WaitForSeconds(11);

            AudioController.Play("NewSpellbook1");
            yield return new WaitForSeconds(12);
        }
       

        // spellBook1
        // wifi3 spellcasting page
        // wifi3("spell casting");

        if (isPlayStory)
        {
            //Spellbook2_third_success
            //AudioController.StopCategory("AlisterVoicesRepeated", 1f);
            //yield return new WaitForSeconds(1);
            AudioController.Play("Spellbook2_third_success");
            yield return new WaitForSeconds(10);
        }


        yield return StartCoroutine(AfterDressingUp());

    }

    // Hidden Tutorial
    IEnumerator FirstInitiateHidden()
    {
        characterNum = 1; // first player starts building character

        isCloakOn = false;
        costumes[1] = null;

        isHatOn = false;
        costumes[0] = null;

        led("M");        
        yield return new WaitForSeconds(1f);
        led("X");
        //AudioController.Play("W");
        //yield return new WaitForSeconds(2);

        AudioController.Play("Water1");

        while (!isCloakOn)
        {

            // Water2_success
            if (costumes[1] == "Water Cloak")
            {
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                led("W");
                wifi1("Water Cloak");
                AudioController.Play("W");
                yield return new WaitForSeconds(2);
                AudioController.Play("Water2_success");
                yield return new WaitForSeconds(13);

                isCloakOn = true;
            }
            // Water2_fail
            else if (costumes[1] != null)
            {
                wifi1("Null Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play(costumes[1].Substring(0, 1));
                costumes[1] = null;

                yield return new WaitForSeconds(2);

                AudioController.Play("Water2_fail");
            }
            // wait
            else
            {
                if (!AudioController.IsPlaying("Water2_wait") && !AudioController.IsPlaying("Water2_fail") && !AudioController.IsPlaying("Water1"))
                {
                    AudioController.Play("Water2_wait");
                }
            }


            yield return new WaitForSeconds(0.5f);
        }

        led("B");
        AudioController.Play("Water3");
        yield return new WaitForSeconds(1f);
        led("w");

        // Hat
        while (!isHatOn)
        {

            // Water4_success
            if (costumes[0] == "Water Hat")
            {
                wifi1("Water Hat");
                led("W");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play("W");
                yield return new WaitForSeconds(2);
                AudioController.Play("Water4_success");
                yield return new WaitForSeconds(11);

                isHatOn = true;
            }
            // Water4_fail
            else if (costumes[0] != null)
            {
                wifi1("Null Hat");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[0].Substring(0, 1));
                costumes[0] = null;

                yield return new WaitForSeconds(2);
                AudioController.Play("Water4_fail");
            }
            // wait
            if (!AudioController.IsPlaying("Water4_wait") && !AudioController.IsPlaying("Water4_fail") && !AudioController.IsPlaying("Water3"))
            {
                AudioController.Play("Water4_wait");
            }

            // prevent crash
            yield return new WaitForSeconds(0.5f);
        }

        witch1 = "Water Witch";

               
        if (isPlayStory)
        {

            AudioController.StopCategory("AlisterVoicesRepeated", 1f);
            yield return new WaitForSeconds(1);

            // Maybe pop out story windows here

            // mirror 1
            AudioController.Play("Mirror1");
            yield return new WaitForSeconds(6);

            //AudioController.Play("WaterHat");
            //yield return new WaitForSeconds(21);
            //AudioController.Play("WaterCloak");
            //yield return new WaitForSeconds(26);
            AudioController.Play("WaterCoven");
            yield return new WaitForSeconds(14);

            AudioController.Play("NewSpellbook1");
            yield return new WaitForSeconds(12);

            AudioController.Play("NewSpellbook2_first_success");
            yield return new WaitForSeconds(12);
        }


        yield return StartCoroutine(SecondInitiateHidden());

    }

    IEnumerator SecondInitiateHidden()
    {
        characterNum = 2; // first player starts building character

        isCloakOn = false;
        costumes[3] = null;

        isHatOn = false;
        costumes[2] = null;

        led("M");
        //AudioController.Play("E");
        yield return new WaitForSeconds(1);
        led("X");
        AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
        AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

        AudioController.Play("NewEarth1");

        while (!isCloakOn)
        {
            // Earth2_success
            if (costumes[3] == "Earth Cloak")
            {
                wifi2("Earth Cloak");
                led("E");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play("E");

                yield return new WaitForSeconds(2);
                AudioController.Play("Earth2_success");
                yield return new WaitForSeconds(12);

                isCloakOn = true;
            }
            // Earth2_fail
            else if (costumes[3] != null)
            {
                wifi2("Null Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[3].Substring(0, 1));
                costumes[3] = null;

                yield return new WaitForSeconds(2);

                AudioController.Play("Earth2_fail");


            }
            else
            {
                if (!AudioController.IsPlaying("Earth2_wait") && !AudioController.IsPlaying("Earth2_fail") && !AudioController.IsPlaying("NewEarth1"))
                {
                    AudioController.Play("Earth2_wait");
                }
            }

            yield return new WaitForSeconds(0.5f);

        }

        // Earth3
        led("B");
        AudioController.Play("Earth3");
        yield return new WaitForSeconds(1);        
        led("e");

        // Hat
        while (!isHatOn)
        {

            // Air4_success
            if (costumes[2] == "Earth Hat")
            {
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                led("E");
                wifi2("Earth Hat");
                AudioController.Play("E");

                yield return new WaitForSeconds(2);
                AudioController.Play("Earth4_success");
                yield return new WaitForSeconds(11); //

                isHatOn = true;
            }
            // Air4_fail
            else if (costumes[2] != null)
            {
                wifi2("Null Hat");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[2].Substring(0, 1));
                costumes[2] = null;

                yield return new WaitForSeconds(2);
                AudioController.Play("Earth4_fail");
            }

            else
            {
                if (!AudioController.IsPlaying("Earth4_wait") && !AudioController.IsPlaying("Earth4_fail") && !AudioController.IsPlaying("Earth3"))
                {
                    AudioController.Play("Earth4_wait");
                }
            }
            yield return new WaitForSeconds(0.5f);
        }

        witch2 = "Earth Witch";

        if (isPlayStory)
        {
            AudioController.StopCategory("AlisterVoicesRepeated", 1f);
            yield return new WaitForSeconds(1);
            AudioController.Play("Mirror1");
            yield return new WaitForSeconds(6);
            //AudioController.Play("EarthHat");
            //yield return new WaitForSeconds(20);
            //AudioController.Play("EarthCloak");
            //yield return new WaitForSeconds(27);
            AudioController.Play("EarthCoven");
            yield return new WaitForSeconds(12);

            AudioController.Play("NewSpellbook1");
            yield return new WaitForSeconds(12);
        
            //Spellbook2_first_success
            //AudioController.StopCategory("AlisterVoicesRepeated", 1f);
            //yield return new WaitForSeconds(1);
            AudioController.Play("NewSpellbook2_second_success");
            yield return new WaitForSeconds(6); //12
        }

        yield return StartCoroutine(ThirdInitiateHidden());
    }

    IEnumerator ThirdInitiateHidden()
    {

        isCloakOn = false;
        costumes[5] = null;

        isHatOn = false;
        costumes[4] = null;

        //AudioController.Play("D");
        //yield return new WaitForSeconds(2);
        AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
        AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
        led("M");
        AudioController.Play("NewDark1");
        yield return new WaitForSeconds(1f);
        led("X");

        characterNum = 3; // first player starts building character

        while (!isCloakOn)
        {
            // Dark_success
            if (costumes[5] == "Darkness Cloak")
            {
                wifi3("Darkness Cloak");
                led("D");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play("D");

                yield return new WaitForSeconds(2);
                AudioController.Play("Dark2_success");
                yield return new WaitForSeconds(10);

                isCloakOn = true;
            }
            // Dark2_fail
            else if (costumes[5] != null)
            {
                costumes[5] = null;
                wifi3("Null Cloak");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[5].Substring(0, 1));
                costumes[5] = null;

                yield return new WaitForSeconds(2);
                AudioController.Play("Dark2_fail");

            }
            else
            {
                if (!AudioController.IsPlaying("Dark2_wait") && !AudioController.IsPlaying("Dark2_fail") && !AudioController.IsPlaying("NewDark1"))
                {
                    AudioController.Play("Dark2_wait");
                }
            }

            yield return new WaitForSeconds(0.5f);
        }

        // Dark3
        led("d");
        AudioController.Play("Dark3");
        yield return new WaitForSeconds(1f);
        led("B");
        // Hat
        while (!isHatOn)
        {
            if (!AudioController.IsPlaying("Dark4_wait") && !AudioController.IsPlaying("Dark4_fail") && !AudioController.IsPlaying("Dark3"))
            {
                AudioController.Play("Dark4_wait");
            }
            // Dark4_success
            if (costumes[4] == "Darkness Hat") //
            {
                wifi3("Darkness Hat");
                led("D");
                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);
                AudioController.Play("D");

                yield return new WaitForSeconds(2);

                AudioController.Play("Dark4_success");
                yield return new WaitForSeconds(9);

                isHatOn = true;
            }
            // Energy4_fail
            else if (costumes[4] != null)
            {
                wifi3("Null Hat");

                AudioController.StopCategory("AlisterVoicesRepeated", 0.5f);
                AudioController.StopCategory("AlisterVoicesOneTime", 0.5f);

                AudioController.Play(costumes[4].Substring(0, 1));

                yield return new WaitForSeconds(2);
                costumes[4] = null;

                AudioController.Play("Dark4_fail");

            }


            yield return new WaitForSeconds(0.5f);
        }

        witch3 = "Dark Witch";

        //EnergyHat

        AudioController.StopCategory("AlisterVoicesRepeated");

        if (isPlayStory)
        {
            AudioController.Play("Mirror1");
            yield return new WaitForSeconds(6);
            //AudioController.Play("DarkHat");
            //yield return new WaitForSeconds(21);
            //AudioController.Play("DarkCloak");
            //yield return new WaitForSeconds(23);
            AudioController.Play("DarkCoven");
            yield return new WaitForSeconds(13);

            AudioController.Play("NewSpellbook1");
            yield return new WaitForSeconds(12);

            AudioController.Play("Spellbook2_third_success");
            yield return new WaitForSeconds(11);
        }
                
        yield return StartCoroutine(AfterDressingUp());

    }

    IEnumerator AfterDressingUp()
    {
        if (isDev)
            chantMode = 0;
        led("Z");
        wifiAll(tutorialName);
        //// turn to the spell casting page

        // wifiNavAll("Follow the directions on your book.");

        if (tutorialName == "The Golden Societies")
        {
            // Spellcast_golden1
            AudioController.Play("NewSpellcast_golden1");
            yield return new WaitForSeconds(22f);
        }
        else
        {
            // Spellcast_hidden1
            AudioController.Play("NewSpellcast_hidden1");
            yield return new WaitForSeconds(28); // 2 second before
        }
            // fade out 

        AudioController.StopMusic(2.0f);
        yield return new WaitForSeconds(2f);

        AudioController.SetGlobalVolume(1f);

        //AudioController.Play("Evil Laugh 2");

        // light and thunder

        //yield return new WaitForSeconds(1);
        led("L");

        AudioController.Play("E");


        //wifiNavAll("Call out the magic words when they appear!");
        //yield return new WaitForSeconds(22f);
        AudioController.SetCategoryVolume("Music", 0.9f);

        AudioController.PlayMusic(tutorialName);
               
    
        wifiGameState("spell casting");
        StartCoroutine(ClearChannel(gameState));

        //AudioController.Play("Open your spellbooks and gather around", 1); // 6s
        //yield return new WaitForSeconds(3);

        //// instructions:
        //AudioController.Play("Follow the directions on the spellbooks");
        ////yield return new WaitForSeconds(6);
      

        StartCoroutine(Left(8.0f));

    }

    IEnumerator Left(float sec = 0)
    {
        yield return new WaitForSeconds(2f);

        wifiNavAll("Circle to your Left!");
        AudioController.Play("Dance_left"); // 4s
        yield return new WaitForSeconds(4.0f);
        led(cauldronLookup["Left"]);
        yield return new WaitForSeconds(sec);
        //if (isWizard)
        //{
        //    yield return new WaitForSeconds(sec);
        //}
        //else
        //{
        //    WaitForArrival();
        //    yield return new WaitForSeconds(sec);
        //}
        yield return StartCoroutine(Stop(1f, 0));

        yield return StartCoroutine(ChantTwoLines(chantMode));

        yield return StartCoroutine(Stop(0.5f, 1));

    }

    IEnumerator Stop(float sec, int c)
    {
        yield return new WaitForEndOfFrame();

        wifiNavAll("Stop");
        wifiGameState("Stop chanting");
        yield return new WaitForSeconds(sec);

                 
        if (c == 1)
            yield return StartCoroutine(CheckPointOneReached());
        else if (c == 2)
            yield return StartCoroutine(CheckPointTwoReached());
        else if (c == 3)
            yield return StartCoroutine(CheckPointThreeReached());
       
        else if (c==4)
            yield return StartCoroutine(CheckPointFourReached());
        else
            yield return null;

        yield return null;

    }

    IEnumerator CheckPointOneReached()
    {
        yield return new WaitForEndOfFrame();
        //wifiNavAll("Arrived");
        AudioController.Play("checkpoint reached");

        //yield return StartCoroutine(Spin(15.0f));
        yield return StartCoroutine(Right(8.0f));
    }
     
    IEnumerator Right(float sec)
    {
        yield return new WaitForEndOfFrame();
        wifiNavAll("Circle to your Right!");
        yield return new WaitForSeconds(0.5f);
        AudioController.Play("Dance_right");

        yield return new WaitForSeconds(4.0f);
        led(cauldronLookup["Right"]);
        yield return new WaitForSeconds(sec);
        //if (isWizard)
        //{
        //    yield return new WaitForSeconds(sec);
        //}
        //else
        //{
        //    WaitForArrival();
        //    yield return new WaitForSeconds(sec);
        //}
        yield return StartCoroutine(Stop(0.5f, 0));
        yield return StartCoroutine(ChantTwoLines(chantMode));

        yield return StartCoroutine(Stop(0.5f, 2));
        yield return null;

    }

    IEnumerator CheckPointTwoReached()
    {
        yield return new WaitForEndOfFrame();
        //wifiNavAll("Arrived");
        AudioController.Play("checkpoint reached");
        yield return new WaitForSeconds(1.5f);

        yield return StartCoroutine(Spin());

    }
    IEnumerator Spin(float sec = 0)
    {
        yield return new WaitForEndOfFrame();
        wifiNavAll("Spin!");
        yield return new WaitForSeconds(0.5f);
        AudioController.Play("Dance_spin");

        isSpin1 = false; isSpin2 = false; isSpin3 = false; isSpinAll = false;
        for(int i = 0; i<4; i++)
        {
            orientations1[i] = false;
            orientations2[i] = false;
            orientations3[i] = false;
        }

        while(!isSpinAll && !isWizard)
        {
            if (!isSpin1)
            {
                yield return null; // return to Update a frame to get latest orientations;
                isSpin1 = orientations1[0];
                for (int i = 1; i < 4; i++)
                {
                    yield return null;
                    isSpin1 = isSpin1 && orientations1[i];
                }

                if (isSpin1)
                {
                    AudioController.Play("checkpoint reached");
                    led(cauldronLookup["Spin1"]);
                    yield return null;
                    if (!isSpin2 && !isSpin3)
                        wifiNav1("Let Initiate 2 & 3 spin!");
                    else if (!isSpin2)
                        wifiNav1("Let Initiate 2 spin!");
                    else if (!isSpin3)
                        wifiNav1("Let Initiate 3 spin!");
                }
            }

            if (!isSpin2)
            {
                yield return null; // return to Update a frame to get latest orientations;
                isSpin2 = orientations2[0];
                for (int i = 1; i < 4; i++)
                {
                    yield return null;
                    isSpin2 = isSpin2 && orientations2[i];
                }

                if (isSpin2)
                {
                    AudioController.Play("checkpoint reached");
                    led(cauldronLookup["Spin2"]);
                    yield return null;
                    if (!isSpin1 && !isSpin3)
                        wifiNav2("Let Initiate 1 & 3 spin!");
                    else if (!isSpin1)
                        wifiNav2("Let Initiate 1 spin!");
                    else if (!isSpin3)
                        wifiNav2("Let Initiate 3 spin!");
                }
            }

            if (!isSpin3)
            {
                yield return null; // return to Update a frame to get latest orientations;
                isSpin3 = orientations3[0];
                for (int i = 1; i < 4; i++)
                {
                    yield return null;
                    isSpin3 = isSpin3 && orientations3[i];
                }

                if (isSpin3)
                {
                    AudioController.Play("checkpoint reached");
                    led(cauldronLookup["Spin3"]);

                    yield return null;
                    if (!isSpin1 && !isSpin2)
                        wifiNav3("Let Initiate 1 & 2 spin!");
                    else if (!isSpin1)
                        wifiNav3("Let Initiate 1 spin!");
                    else if (!isSpin2)
                        wifiNav3("Let Initiate 2 spin!");
                }
            }

            isSpinAll = isSpin1 && isSpin2 && isSpin3;
            yield return new WaitForSeconds(0.2f);
            if (isSpinAll || isWizard)
            {
                wifiGameState("Stop spinning");
            }
            yield return new WaitForSeconds(0.2f); // return to Update frames for 1f;

        }
        wifiGameState("Stop spinning");
        yield return StartCoroutine(Stop(0f, 3));

        yield return StartCoroutine(ChantTwoLines(chantMode));

        yield return StartCoroutine(Stop(0f, 3));

    }
    IEnumerator CheckPointThreeReached()
    {
        yield return new WaitForEndOfFrame();
        // wifiNavAll("Arrived");
        // AudioController.Play("spell ready 1");
        yield return StartCoroutine(Center(5f));
        yield return null;

    }
    IEnumerator CheckPointFourReached()
    {
        yield return new WaitForEndOfFrame();
        //wifiNavAll("Arrived");
        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(Ending());

        yield return null;


    }
    IEnumerator Center(float sec = 0)
    {
        yield return new WaitForEndOfFrame();
        wifiNavAll("Move to the Center!");
        AudioController.Play("Dance_center");
        led(cauldronLookup["Center"]);

        yield return new WaitForSeconds(5f);
        yield return new WaitForSeconds(sec);
        yield return StartCoroutine(Stop(0f, 0));

        yield return StartCoroutine(ChantOneLine());

        yield return StartCoroutine(Stop(0f, 0));

        yield return StartCoroutine(Raise());
        yield return StartCoroutine(Stop(0f, 0));

        yield return null;

    }

    IEnumerator Raise(float sec = 0)
    {
        yield return new WaitForEndOfFrame();
        wifiNavAll("Raise your spellbooks!");

        AudioController.Play("Dance_raise");
        yield return new WaitForSeconds(3f);

        // prevent volume from increasing too quickly
        // stop dance music         

        if (AudioController.IsPlaying(tutorialName)){
            AudioController.StopMusic(3);
        }
        yield return new WaitForSeconds(3f);

        AudioController.SetGlobalVolume(1f);

        if (tutorialName == "The Golden Societies")
        {

            led(cauldronLookup["GoldenLightning"]);
            AudioController.Play("E");
        }
        else
        {
            led(cauldronLookup["HiddenLightning"]);

            AudioController.Play("D");
        }

        // yield return new WaitForSeconds(sec);
        //isRaising = false;
        //if (!isWizard && isDepth)
        //{
        //    while (!isRaising)
        //    {
        //        yield return null;
        //        if (depthController.isHigh)
        //            isRaising = true;
        //        yield return new WaitForSeconds(0.5f);

        //    }

        //}

        //else
        //Debug.Log("here! raising!");

        yield return StartCoroutine(Stop(0f, 0));

        yield return StartCoroutine(Ending());
        yield return null;


    }


    IEnumerator ChantOneLine(float sec = 6)
    {
        wifiGameState("Unison");
        wifiNavAll("Stop"); // stop showing dance text
        wifiAll("...");

        AudioController.Play("Words_unison");
        for (int j = 0; j < 20; j++)
        {

            float v = 0.5f + j / 40;
            AudioController.SetCategoryVolume("Music", v);
            yield return new WaitForSeconds(0.3f);
        }

        string s = "We open the circle of " + tutorialName + "!";
        wifiAll(s);

        yield return new WaitForSeconds(1.0f);



        if (isWizard)
            yield return new WaitForSeconds(chantLineTime);
        else
        {
            bool isNext = false;
            
            while (isNext == false && !isWizard)
            {

                if (ld1 > loudnessThreshold && ld2 > loudnessThreshold && ld3 > loudnessThreshold)
                {
                    isNext = true;
                }
                
                yield return new WaitForSeconds(0.2f); // needed for while loops
            }

            AudioController.Play("spell ready 1");
            if (tutorialName == "The Golden Societies")
            {
                led(cauldronLookup["GoldenCircle"]);
            }
            else
            {
                led(cauldronLookup["HiddenCircle"]);
            }
            for (int j = 0; j < 20; j++)
            {

                float v = 1 - j / 40;
                AudioController.SetCategoryVolume("Music", v);
                yield return new WaitForSeconds(0.2f);
            }
        }

        yield return null;

    }

    IEnumerator ChantTwoLines(int mode = 0)  // mode = 0 Unison, 1 Call and Response
    {
        wifiAll("...");
        // prevent volume from dropping too quickly            
        float vol = AudioController.GetCategoryVolume("Music");
        Debug.Log(vol);
        if (mode == 0)
        {
            wifiGameState("Unison");            

            AudioController.Play("Words_unison");
            for (int j = 0; j < 20; j++)
            {

                float v = 0.5f + j / 40;
                AudioController.SetCategoryVolume("Music", v);
                yield return new WaitForSeconds(0.3f);
            }

            for (int i = 0; i < 2; i++)
            {
                wifiAll(unison[chantLine]);
                yield return new WaitForSeconds(1.0f); // appear for a while

                if (isWizard)
                    yield return new WaitForSeconds(chantLineTime);
                else
                {
                    bool isNext = false;
                    float waitedTime = 0f;
                    while (isNext == false && !isWizard)
                    {

                        if (ld1 > loudnessThreshold && ld2 > loudnessThreshold && ld3 > loudnessThreshold)
                        {
                            isNext = true;
                        }
                        else
                        {
                            yield return new WaitForSeconds(0.5f);
                            waitedTime = waitedTime + 0.5f;
                        }

                        yield return null; // needed for while loops
                    }

                    // light up the caldron
                    led(cauldronLookup[unison[chantLine]]);
                                        
                    if (waitedTime < chantLineTime)
                    {
                        yield return new WaitForSeconds(chantLineTime - waitedTime);
                    }
                    else
                        yield return new WaitForSeconds(0.5f);
                }

                chantLine += 1;
                
                if (chantLine == 6)
                {
                    break;
                }


            }
        }
        else if (mode == 1)
        {
            wifiGameState("Call and Response");

            AudioController.Play("Words_call-response");

            for(int j=0; j<20; j++){

                float v = 0.5f + j / 40;
                AudioController.SetCategoryVolume("Music", v);
                yield return new WaitForSeconds(0.3f);
            }            
            

            for (int i = 0; i < 2; i++)
            {
                bool isNext = false;
                bool condition = false;
                string l = "";
                float waitedTime = 0f;

                // send out each player's script
                for (int j = 1; j <= 3; j++)
                {
                    wifi(j, call_response[chantLine, j - 1]);     
                    if(call_response[chantLine, j - 1] != "...")
                    {
                        l = call_response[chantLine, j - 1];
                    }               

                } // end j loop
                yield return new WaitForSeconds(2.0f);


                if (isWizard)
                    yield return new WaitForSeconds(chantLineTime);
                else
                {

                    while (isNext == false && !isWizard)
                    {
                        if (chantLine == 0)
                        {
                            condition = (ld1 > loudnessThreshold);
                        }
                        else if(chantLine == 1)
                        {
                            condition = ((ld2 > loudnessThreshold) && (ld3 > loudnessThreshold));
                        }
                        else if(chantLine == 2)
                        {
                            condition = (ld2 > loudnessThreshold) ;
                        }
                        else if (chantLine == 3)
                        {
                            condition = ( (ld1 > loudnessThreshold) && (ld3 > loudnessThreshold) );
                        }
                        else if (chantLine == 4)
                        {
                            condition = (ld3 > loudnessThreshold);
                        }
                        else if (chantLine == 5)
                        {
                            condition = ((ld1 > loudnessThreshold) && (ld2 > loudnessThreshold));
                        }

                        if (condition)
                        {
                            isNext = true;
                        }
                        else
                        {
                            yield return new WaitForSeconds(1.0f);
                            waitedTime = waitedTime + 1;
                        }

                        yield return new WaitForSeconds(0.2f);
                    }
                    
                    led(cauldronLookup[l]);
                }


                if (waitedTime < chantLineTime)
                {
                    yield return new WaitForSeconds( (chantLineTime - waitedTime));
                }

                if(i == 0)
                {
                    yield return new WaitForSeconds(2.0f);
                }
                chantLine += 1;
                                
                if (chantLine == 6)
                {
                    break;
                }
            }
        }

        for (int j = 0; j < 20; j++)
        {
            float v = 1 - j / 40;
            AudioController.SetCategoryVolume("Music", v);
            yield return new WaitForSeconds(0.2f);
        }
        wifiAll("clear");
        yield return null;
    }


    void DetectSpin()
    {        
        euler1 = q1.eulerAngles;

        if (euler1.z < 90)
            orientations1[0] = true;
        else if (euler1.z < 180)
            orientations1[1] = true;
        else if (euler1.z < 270)
            orientations1[2] = true;
        else
            orientations1[3] = true;

        // gyro 2
        euler2 = q2.eulerAngles;

        if (euler2.z < 90)
            orientations2[0] = true;
        else if (euler2.z < 180)
            orientations2[1] = true;
        else if (euler2.z < 270)
            orientations2[2] = true;
        else
            orientations2[3] = true;

        // gyro 3
        euler3 = q3.eulerAngles;

        if (euler3.z < 90)
            orientations3[0] = true;
        else if (euler3.z < 180)
            orientations3[1] = true;
        else if (euler3.z < 270)
            orientations3[2] = true;
        else
            orientations3[3] = true;
    }

    void WaitForRaising()
    {
        float waitedTime = 0f;
        while (isRaising == false && isWizard == false)
        {
            waitedTime = waitedTime + 0.5f;
            //WaitForSecs(0.5f);
        }

    }

    void WaitForArrival()
    {

    }

    // tutorials
    IEnumerator Ending()
    {
        yield return new WaitForSeconds(4f);
        // play spell ready sfx
        AudioController.Play("spell ready 2");
        yield return new WaitForSeconds(4f);

        //// turn to the result page
        wifiGameState("result");        
        StartCoroutine(ClearChannel(gameState));

        if (isTutorial && tutorialName == "The Golden Societies")
        {
            led("G");

            AudioController.Play("golden 2");
            yield return new WaitForSeconds(4f);

            AudioController.Play("Spellcast_golden2");
            yield return new WaitForSeconds(38);

            AudioController.Play("victory");
            yield return new WaitForSeconds(10.0f);

            tutorialName = "The Hidden Orders";


        }

        else if (isTutorial && tutorialName == "The Hidden Orders")
        {
            led("H");

            AudioController.Play("hidden 1");
            yield return new WaitForSeconds(5f);

            AudioController.Play("Spellcast_hidden2");
            yield return new WaitForSeconds(38);

            AudioController.Play("victory");
            yield return new WaitForSeconds(10.0f);

           tutorialName = "The Golden Societies";

        }


        else
        {
            
        }

        isReset = true;
        yield return new WaitForSeconds(1.0f);

        yield return StartCoroutine(Starting());


    }

    // lookup RFID-costume dictionary, send commands to LED, play effect audio, update spell book
    void OnCostumeScanned(string costumeID)
    {
        itemScanned = costumeLookup[costumeID];
        string element = itemScanned.Substring(0, 1);

        // tutorial mode
        if (isTutorial)
        {
            // in tutorial, players cannot scan the hat before the cloak
            if (!isCloakOn && itemScanned.Contains("Hat"))
                return;

            // no need to de-selecting, register itemScanned directly to costumes[]
            registerCostume(characterNum); // first player starts building character


            return;
        }


        //Debug.Log(itemScanned);

        // will keep sending and crash?
        led(element);

        // Debug.Log("Scanned:" + itemScanned + ", send element:" + element);

        // check if rescanning/deselecting the item

        itemsSelected = 0;
        for (int i = 0; i < 6; i++)
        {
            if (costumes[i] == itemScanned)
            {
                //Debug.Log ("Deselecting " + itemScanned);
                costumes[i] = null;

                // play de-selecting audio
                // nullify corresponding costume, send nullifying msg correspondingly                
                if (i == 0)
                {
                    witch1 = null;
                    wifi1("Null Hat");
                }
                else if (i == 1)
                {
                    witch1 = null;
                    wifi1("Null Cloak");
                }
                else if (i == 2)
                {
                    witch2 = null;
                    wifi2("Null Hat");
                }
                else if (i == 3)
                {
                    witch2 = null;
                    wifi2("Null Cloak");
                }
                else if (i == 4)
                {
                    witch3 = null;
                    wifi3("Null Hat");
                }
                else
                {
                    witch3 = null;
                    wifi3("Null Cloak");
                }

                return;
            }

            if (costumes[i] != null)
                itemsSelected++;
        }

        // if attempting to select new item after 6 items have been selected, break
        if (itemsSelected == 6)
            return;

        // selecting a new item - determine if user scanned a hat or cloak 
        // 		then, update first empty corresponding location in costumes array (first hat scanned redBook= witch1), send to phone player 1
        //		Note: hats in costumes 0,2,4 -- cloaks in 1,3,5 (witch1 = 0,1)


        // Play element sound
        // AudioController.Play(element);

        int costumeIndex = -1;
        registerCostume(0);

        // if there are no available hat/cloak spots, cannot add new item, return
        if (costumeIndex == -1)
        {
            if (itemScanned.Contains("Hat"))
                Debug.Log("3 Hats have already been selected");
            else
                Debug.Log("3 Cloaks have already been selected");
            return;
        }

        // update witch values corresponding to costume array (ex: witch1 = hat in costumes[0] & cloak in costumes[1])
        // the witch name on the spell book will be determined on the phone, not by receving msg from here, 
        // otherwise we need another channel to avoid overlapping with costume msgs.
        updateWitchIdentities();

    }

    void registerCostume(int n)
    {

        if (isTutorial)
        {
            if (n < 1 || n > 3)
                return;
            if (itemScanned.Contains("Hat"))
            {
                if (costumes[2 * n - 2] == null)
                {
                    costumes[2 * n - 2] = itemScanned;
                }

            }
            if (itemScanned.Contains("Cloak"))
            {
                if (costumes[2 * n - 1] == null)
                {
                    costumes[2 * n - 1] = itemScanned;
                }
            }
        }

        else
        {
            if (itemScanned.Contains("Hat"))
            {
                if (costumes[0] == null)
                {
                    costumes[0] = itemScanned;
                    costumeIndex = 0;

                    wifi1(itemScanned);

                }
                else if (costumes[2] == null)
                {
                    costumes[2] = itemScanned;
                    costumeIndex = 2;

                    wifi2(itemScanned);

                }
                else if (costumes[4] == null)
                {
                    costumes[4] = itemScanned;
                    costumeIndex = 4;
                    wifi3(itemScanned);
                }
            }
            else if (itemScanned.Contains("Cloak"))
            {
                if (costumes[1] == null)
                {
                    costumes[1] = itemScanned;
                    costumeIndex = 1;
                    wifi1(itemScanned);
                }
                else if (costumes[3] == null)
                {
                    costumes[3] = itemScanned;
                    costumeIndex = 3;
                    wifi2(itemScanned);
                }
                else if (costumes[5] == null)
                {
                    costumes[5] = itemScanned;
                    costumeIndex = 5;
                    wifi3(itemScanned);
                }
            }
        }

    }
    void checkDeselecting()
    {

    }
    void updateWitchIdentities()
    {
        if ((costumeIndex == 0 || costumeIndex == 1) && (costumes[0] != null && costumes[1] != null))
        {
            if (witchLookup.ContainsKey(costumes[0].Substring(0, 1) + costumes[1].Substring(0, 1)))
                witch1 = witchLookup[costumes[0].Substring(0, 1) + costumes[1].Substring(0, 1)];
            else
                witch1 = witchLookup[costumes[1].Substring(0, 1) + costumes[0].Substring(0, 1)];

        }
        else if ((costumeIndex == 2 || costumeIndex == 3) && (costumes[2] != null && costumes[3] != null))
        {
            if (witchLookup.ContainsKey(costumes[2].Substring(0, 1) + costumes[3].Substring(0, 1)))
                witch2 = witchLookup[costumes[2].Substring(0, 1) + costumes[3].Substring(0, 1)];
            else
                witch2 = witchLookup[costumes[3].Substring(0, 1) + costumes[2].Substring(0, 1)];

        }
        else if ((costumeIndex == 4 || costumeIndex == 5) && (costumes[4] != null && costumes[5] != null))
        {
            if (witchLookup.ContainsKey(costumes[4].Substring(0, 1) + costumes[5].Substring(0, 1)))
                witch3 = witchLookup[costumes[4].Substring(0, 1) + costumes[5].Substring(0, 1)];
            else
                witch3 = witchLookup[costumes[5].Substring(0, 1) + costumes[4].Substring(0, 1)];
        }
    }

    void OnGUI()
    {      
         
        GUILayout.BeginVertical();

        // gameStateDisplay.text = "Game State: " + gameStateMsg;
        loudnessText.text = "ld1: " + ld1.ToString() + ", ld2: " + ld2.ToString() + ", ld3: " + ld3.ToString() + ", ld_thresh: " + loudnessThreshold;
        //gyroText.text = "g1w: " + g1w.ToString() + ", g2w: " + g2w.ToString() + ", g3w: " + g3w.ToString();
        eulerText.text = "g1: " + euler1.z + "g2: " + euler2.z + "g3: " + euler3.z;
        //gyroText.text = "g1w: " + g1w.ToString() + ", g1x: " + g1x.ToString() + ", g1y: " + g1y.ToString() + ", g1z: " + g1z.ToString();


        wifiMsgDisplay1.text = "Wifi1: " + wifiMsg1;
        wifiMsgDisplay2.text = "Wifi2: " + wifiMsg2;
        wifiMsgDisplay3.text = "Wifi3: " + wifiMsg3;
        navDisplay1.text = "Nav1: " + navMsg1;
        navDisplay2.text = "Nav2: " + navMsg2;
        navDisplay3.text = "Nav3: " + navMsg3;
        ledMsgDisplay.text = "LED: " + ledMsg;

        trackingDiplay1.text = tracking1; //+ "curPos_red: " + curPos_red.ToString();
        trackingDiplay2.text = tracking2; //+ "curPos_yellow: " + curPos_yellow.ToString();
        trackingDiplay3.text = tracking3; //+ "curPos_green: " + curPos_green.ToString();

        GUILayout.EndVertical();

        // timer to display some messages for certain seconds
    }

    void getLoudness1(IntBackchannelType c)
    {
        ld1 = c.INT_VALUE;
    }
    void getLoudness2(IntBackchannelType c)
    {
        ld2 = c.INT_VALUE;

    }
    void getLoudness3(IntBackchannelType c)
    {
        ld3 = c.INT_VALUE;
    }
    void getGyro1(GyroControllerType c)
    {
        q1.w = c.GYRO_W;
        q1.x = c.GYRO_X;
        q1.y = c.GYRO_Y;
        q1.z = c.GYRO_Z;
    }
    void getGyro2(GyroControllerType c)
    {
        q2.w = c.GYRO_W;
        q2.x = c.GYRO_X;
        q2.y = c.GYRO_Y;
        q2.z = c.GYRO_Z;
    }
    void getGyro3(GyroControllerType c)
    {
        q3.w = c.GYRO_W;
        q3.x = c.GYRO_X;
        q3.y = c.GYRO_Y;
        q3.z = c.GYRO_Z;
    }   


    void keyboardControls()
    {
        // for debug purposes
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            wifiNavAll("Circle to your Left");
            //serialControllerLED.SendSerialMessage("A");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            wifiGameState("Unison");
            //serialControllerLED.SendSerialMessage("A");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            wifiNavAll("Circle to your Right");
            //serialControllerLED.SendSerialMessage("A");
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            wifiGameState("Call and Response");
        }
        // for debug purposes
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            wifiNavAll("Spin!");
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            wifiNavAll("Center");
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            wifiNavAll("Raise");
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            wifiGameState("Stop chanting");
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            wifiGameState("Stop spinning");
        }               

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            wifiAll("reset");
        }


        // tutorials
        if (Input.GetKeyDown(KeyCode.Greater))
        {
            wifiAll("The Golden Societies");
        }
        if (Input.GetKeyDown(KeyCode.Less))
        {
            wifiAll("The Hidden Orders");
        }

        // pages
        if (Input.GetKeyDown(KeyCode.F1))
        {
            wifiGameState("journal");
            StartCoroutine(ClearChannel(gameState));
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            wifiGameState("scan item");
            StartCoroutine(ClearChannel(gameState));
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            wifiGameState("spell casting");
            StartCoroutine(ClearChannel(gameState));
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            wifiGameState("result");
            StartCoroutine(ClearChannel(gameState));
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            wifiGameState("reload");
            StartCoroutine(ClearChannel(gameState));
        }


        // Wizard of OZ
        if (Input.GetKeyDown(KeyCode.J))
        {
            isWizard = true;
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            isWizard = false;
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            loudnessThreshold += 10;
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            loudnessThreshold -= 10;
        }



        // LED test
        if (Input.GetKeyDown(KeyCode.A))
        {
            led("A");
        }                
        if (Input.GetKeyDown(KeyCode.F))
        {
            led("F");
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            led("D");
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            led("W");
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            led("N");     // Brown
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            led("B");   // + brightness
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            led("M");      // - brightness
        }
                
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            led("L");
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            led("R");
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            led("Y");
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            led("I");
        }
        if (Input.GetKeyDown(KeyCode.Keypad4)) // brown
        {
            led("s");
        }
        if (Input.GetKeyDown(KeyCode.Keypad5)) 
        {
            led("p");
        }
        if (Input.GetKeyDown(KeyCode.Minus)) // decrease lightning StrikeTimes by 50
        {
            led("n");
        }
        if(Input.GetKeyDown(KeyCode.Plus)) // increase lightning StrikeTimes by 50
        {
            led("b");
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            led("X");  //           rainbow(20);
        }
        if (Input.GetKeyDown(KeyCode.Keypad7))
        {
            led("O"); // rainbowCycle(20);
        }
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            led("S"); //           theaterChaseRainbow(20);
        }
        if (Input.GetKeyDown(KeyCode.Keypad9)) // TURN OFF
        {
            led("Z");
        }




        

    }

    
    public IEnumerator ClearChannel(StringServerBackchannel c)
    {
        yield return new WaitForSeconds(0.25f); // wait for a little bit
        c.setValue("clear");

        yield return new WaitForEndOfFrame();
    }
    public IEnumerator ClearSerialLED(SerialController s) 
    {
        yield return new WaitForSeconds(0.1f); 
        serialControllerLED.SendSerialMessage("C");
        yield return new WaitForEndOfFrame();
    }


    void wifi1(string s)
    {              
        msg1.setValue(s);
        // StartCoroutine(ClearChannel(msg1)); // if too fast, it will not be received
    }
    void wifi2(string s)
    {
        msg2.setValue(s);

        //StartCoroutine(ClearChannel(msg2));
    }
    void wifi3(string s)
    {        
        msg3.setValue(s);
        //StartCoroutine(ClearChannel(msg3));
    }
   
    void wifi(string player, string s)
    {
        if (player == "red")
            wifi1(s);
        else if (player == "yellow")
            wifi2(s);
        else if (player == "green")
            wifi3(s);
        else
            return;


    }
    void wifi(int i, string s)
    {
        if (i == 1)
            wifi1(s);
        else if (i == 2)
            wifi2(s);
        else if (i == 3)
            wifi3(s);
        else
            return;
    }
    void wifiAll(string s)
    {
        wifi1(s);
        wifi2(s);
        wifi3(s);
        // spell = s;
    }

    // send wifi messages and visualize
    void wifiNav1(string s)
    {
        if (isWifi && isRedBook)
        {
            navMsg1 = s;
            nav1.setValue(navMsg1);
            //StartCoroutine(ClearChannel(nav1));
        }
    }
    void wifiNav2(string s)
    {
        if (isWifi && isYellowBook)
        {
            navMsg2 = s;
            nav2.setValue(navMsg2);
            //StartCoroutine(ClearChannel(nav2));
        }
    }
    void wifiNav3(string s)
    {
        if (isWifi && isGreenBook)
        {
            navMsg3 = s;
            nav3.setValue(navMsg3);
            //StartCoroutine(ClearChannel(nav3));
        }
    }
    void wifiNav(string player, string s)
    {
        if (player == "red")
            wifiNav1(s);
        else if (player == "yellow")
            wifiNav2(s);
        else if (player == "green")
            wifiNav3(s);
        else
            return;
    }
    void wifiNav(int j, string s)
    {
        if (j == 1)
            wifiNav1(s);
        else if (j == 2)
            wifiNav2(s);
        else if (j == 3)
            wifiNav3(s);
        else
            return;
    }
    void wifiNavAll(string s)
    {
        wifiNav1(s);
        wifiNav2(s);
        wifiNav3(s);
        // gameStateMsg = s;
    }
    void wifiGameState(string s)
    {
        if (isWifi)
        {
            gameState.setValue(s);
            gameStateMsg = s;
            
        }

    }

    void led(string s)
    {
        if (isSerialLED)
        {
            ledMsg = s;
            serialControllerLED.SendSerialMessage(ledMsg);
            //StartCoroutine(ClearSerialLED(serialControllerLED));

        }

    }
}