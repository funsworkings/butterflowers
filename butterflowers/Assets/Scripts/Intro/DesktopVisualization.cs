using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

using UnityEngine.Events;
using UIExt.Behaviors.Visibility;
using System.Linq;

namespace Intro {

    public class DesktopVisualization: MonoBehaviour {

        public UnityEvent onBegin, onIntroComplete, onBeginSpawnFiles, onSpawnFile, onEndSpawnFiles, onSpawnNest, onConstructRoom, onBeginWipe, onEndWipe, onComplete;
        public TextureCollection StarterPack;

        [Header("Timing")]
        public float introDelay = 1f;
        public float welcomeTextDelay = 1f;
        public float beaconSpawnDelay = 1f;
        public float nestDropDelay = 1f;
        public float maxTimeBetweenSpawnFiles = 1f;
        public float nestOverflowTextDelay = 1f;
        public float wipeObjectsDelay = 1f;
        public float maxTimeWipeObjects = 1f;

        [Header("Tutorial")]
        public bool hasClicked = false;
        public bool hasFocused = false, hasLostFocus = false;
        public bool hasAdded = false;
        public bool hasSpilled = false;
        public bool hasWipedButterflies = false;

        [Header("UI elements")]
            public ToggleOpacity uiOpacity;
            public TogglePosition welcomeText;
            public TogglePosition clickText;
            public TogglePosition holdText, loseHoldText;
            public TogglePosition addBeacontext;
            public TogglePosition nestOverflowText;

        [Header("Scene objects")]
            public GameObject floor;
            public Nest nest;
            Rigidbody nest_rigid;

            public Unknown unknown;
            public Quilt quilt;
            public Scribe terminal;
            public FileNavigator Files;
            public MotherOfButterflies mother;

        [Header("Spawn beacons")]
            public GameObject pr_beacon;
            public Transform nestRoot, desktopRoot;
            public float desktopRadius = 1f, stepRadius = 1f;
            public int itemsPerRing = 6, stepItems = 1;
            public List<GameObject> files = new List<GameObject>();
            Dictionary<Beacon, Texture2D> file_lookup = new Dictionary<Beacon, Texture2D>();


        #region Monobehaviour callbacks

        void OnEnable()
        {
            nest.onAddBeacon += onIngestBeacon;
            nest.onRemoveBeacon += onReleaseBeacon;

            Events.onFireEvent += onReceiveEvent;

            Files.onRefresh += onRefreshFiles;
        }

        void OnDisable()
        {
            nest.onAddBeacon -= onIngestBeacon;
            nest.onRemoveBeacon -= onReleaseBeacon;

            Events.onFireEvent -= onReceiveEvent;

            Files.onRefresh -= onRefreshFiles;
        }

        #endregion

        public void Begin() 
        {
            onBegin.Invoke();
            StartCoroutine("Intro");
        }

        IEnumerator Intro()
        {
            yield return new WaitForSeconds(introDelay);

            uiOpacity.Show();
            while (!uiOpacity.Visible) yield return null;

            welcomeText.Show();
            yield return new WaitForSeconds(welcomeTextDelay);
            welcomeText.Hide();


            terminalinprogress = true;
            TerminalIntro();
            while (terminalinprogress) yield return null;
            onIntroComplete.Invoke();


            onBeginSpawnFiles.Invoke();
            yield return new WaitForSeconds(beaconSpawnDelay);

            SpawnDesktopFiles();
            while (!spawnFiles) yield return null;
            onEndSpawnFiles.Invoke();

            PushToTerminal("NETWORK FILES WAS SCRAPED!");
            yield return new WaitForSeconds(.8f);

            PushToTerminal("<i>butterflowers</i> WAS BOOTED!");
            yield return new WaitForSeconds(.2f);


            SpawnNest();
            yield return new WaitForSeconds(nestDropDelay);
            onConstructRoom.Invoke();

            hasClicked = false;
            while (!hasClicked) 
            {
                clickText.Show();
                yield return null;
            }
            clickText.Hide();

            hasFocused = false;
            while (!hasFocused) 
            {
                holdText.Show();
                yield return null;
            }
            holdText.Hide();

            hasAdded = false;
            while (!hasAdded) {
                addBeacontext.Show();
                yield return null;
            }
            addBeacontext.Hide();

            hasSpilled = false;
            while (!hasSpilled) yield return null;
            nestOverflowText.Show();
            yield return new WaitForSeconds(nestOverflowTextDelay);
            nestOverflowText.Hide();

            hasLostFocus = false;
            while (!hasLostFocus) 
            {
                loseHoldText.Show();
                yield return null;
            }
            loseHoldText.Hide();

            yield return new WaitForSeconds(3f);

            onBeginWipe.Invoke();
            yield return new WaitForSeconds(wipeObjectsDelay);

            float delay = 0f;

            quilt.gameObject.SetActive(false);

            delay = Random.Range(0f, maxTimeWipeObjects);
            yield return new WaitForSeconds(delay);

            nest.gameObject.SetActive(false);

            delay = Random.Range(0f, maxTimeWipeObjects);
            yield return new WaitForSeconds(delay);

            foreach (GameObject b in files) {
                b.SetActive(false);
                Events.ReceiveEvent(EVENTCODE.BEACONDELETE, AGENT.World, AGENT.Beacon, details: file_lookup[b.GetComponent<Beacon>()].name);

                delay = Random.Range(0f, maxTimeWipeObjects / 3f);
                yield return new WaitForSeconds(delay);
            }

            delay = Random.Range(0f, maxTimeWipeObjects);
            yield return new WaitForSeconds(delay);

            floor.SetActive(false);
            onEndWipe.Invoke();

            yield return new WaitForSeconds(1f);
            KillAllButterflies();
            while (!hasWipedButterflies) yield return null;

            onComplete.Invoke();
        }

        #region Room construction

        public void SpawnNest()
        {
            nest_rigid = nest.GetComponent<Rigidbody>();
            nest_rigid.useGravity = true;

            onSpawnNest.Invoke();
        }

		public void SpawnDesktopFiles()
        {
            var files = StarterPack.elements;
            StartCoroutine(SpawningDesktopFiles(files, desktopRoot));
        }

        bool spawnFiles = false;
        IEnumerator SpawningDesktopFiles(Texture2D[] files, Transform root)
        {
            float radius = desktopRadius;
            int items = 0;
            int itemCap = itemsPerRing;
            int level = 1;
            float angle = 0f;

            for (int i = 0; i < files.Length; i++) 
            {
                radius = level * desktopRadius + (level - 1) * stepRadius;

                angle = ((float)items / (Mathf.Min(itemCap, files.Length-itemCap) + 1) ) * 2f * Mathf.PI;

                var x = Mathf.Cos(angle);
                var y = Mathf.Sin(angle);

                var path = files[i].name;
                var pos = root.TransformPoint(new Vector3(x, 0f, y) * radius);

                var file = SpawnBeacon(pos, root);
                file.file = path;
                file.fileEntry = null;
                file.origin = pos;
                file.type = Beacon.Type.External;

                this.files.Add(file.gameObject);
                file_lookup.Add(file, files[i]);

                onSpawnFile.Invoke();
                Events.ReceiveEvent(EVENTCODE.BEACONADD, AGENT.World, AGENT.Beacon, details: path);

                if (++items > itemCap) 
                {
                    ++level;
                    
                    items = 0;
                    itemCap = level * itemsPerRing + ((level - 1) * stepItems);
                }

                yield return new WaitForSeconds(Random.Range(0f, maxTimeBetweenSpawnFiles));
            }

            spawnFiles = true;
        }

        Beacon SpawnBeacon(Vector3 position, Transform root)
        {
            GameObject beacon = Instantiate(pr_beacon, root);

            beacon.transform.position = position;
            beacon.transform.up = root.up;

            return beacon.GetComponent<Beacon>();
        }

		#endregion


        public void KillAllButterflies()
        {
            mother.KillButterflies(false);
            WaitForComplete();
        }

        public void WaitForComplete()
        {
            StartCoroutine(WaitForButterflyDeath());
        }

        IEnumerator WaitForButterflyDeath()
        {
            yield return new WaitForSeconds(3.33f);
            hasWipedButterflies = true;
        }

        #region Console

        public void RefreshDesktopFiles()
        {
            Files.Refresh();
        }

        void onRefreshFiles()
        {
            var entries = Files.GetFiles();
            var files = Files.GetPathsFromFiles(entries);
            StartCoroutine("RefreshingDesktop", files);
        }

        bool refreshing = true;
        IEnumerator RefreshingDesktop(string[] files)
        {
            var root = Files.Path;

            for (int i = 0; i < files.Length; i++) {
                Events.ReceiveEvent(EVENTCODE.DISCOVERY, AGENT.World, AGENT.Beacon, root + "/" + files[i]);
                yield return new WaitForSeconds(.05f);
            }

            refreshing = false;
        }

        public void TerminalIntro()
        {
            StartCoroutine("TerminalIntroSequence");
        }

        bool terminalinprogress = false;
        IEnumerator TerminalIntroSequence()
        {
            int j = 0;

            PushToTerminal("BOOTING UP <i>butterflowers</i>...");
            yield return new WaitForSeconds(1f);

            PushToTerminal("SPAWNING <i>VOID</i>...");
            yield return new WaitForSeconds(.8f);
            for (j = 0; j < 16; j++) 
            {
                PushToTerminal(unknown.none());
                yield return new WaitForSeconds(.087f);
            }

            PushToTerminal("<i>VOID</i> WAS SPAWNED!");
            yield return new WaitForSeconds(.2f);

            PushToTerminal("SPAWNING <i>NOISE</i>...");
            yield return new WaitForSeconds(.8f);
            for (j = 0; j < 16; j++) {
                PushToTerminal(unknown.perlin());
                yield return new WaitForSeconds(.087f);
            }

            PushToTerminal("<i>NOISE</i> WAS SPAWNED!");
            yield return new WaitForSeconds(.2f);

            PushToTerminal("SPAWNING <i>RIVERS</i>...");
            yield return new WaitForSeconds(1.2f);
            for (j = 0; j < 16; j++) {
                PushToTerminal(unknown.river());
                yield return new WaitForSeconds(.087f);
            }

            PushToTerminal("<i>RIVERS</i> WAS SPAWNED!");
            yield return new WaitForSeconds(.2f);

            PushToTerminal("SPAWNING <i>CAMERA FEED</i>...");
            yield return new WaitForSeconds(2f);
            for (j = 0; j < 16; j++) {
                PushToTerminal(unknown.feed());
                yield return new WaitForSeconds(.087f);
            }

            PushToTerminal("<i>CAMERA FEED</i> WAS SPAWNED!");
            yield return new WaitForSeconds(.2f);

            PushToTerminal("LOADING USER DESKTOP FILES...");
            yield return new WaitForSeconds(2.3f);

            RefreshDesktopFiles();
            while (refreshing) yield return null;

            PushToTerminal("USER DESKTOP FILES WAS LOADED!");
            yield return new WaitForSeconds(.2f);

            PushToTerminal("SCRAPING NETWORK FILES...");
            yield return new WaitForSeconds(.8f);

            terminalinprogress = false;
        }

		#endregion

		#region Nest callbacks

		void onIngestBeacon(Beacon beacon)
        {
            beacon.visible = false;

            if (beacon.type == Beacon.Type.None) return;

            var file = beacon.file;
            var tex = file_lookup[beacon];

            quilt.Add(tex);
        }

        void onReleaseBeacon(Beacon beacon)
        {
            beacon.visible = true;

            if (beacon.type == Beacon.Type.None) return;

            var file = beacon.file;
            quilt.Pop(file);
        }

        #endregion

        public void OnFocus()
        {
            hasFocused = true;
        }

        public void OnLostFocus()
        {
            hasLostFocus = true;
        }

        public void PushToTerminal(string text)
        {
            Events.ReceiveEvent(EVENTCODE.UNKNOWN, AGENT.NULL, AGENT.NULL, text);
        }

        void onReceiveEvent(EVENTCODE @event, AGENT a, AGENT b, string details)
        {
            if (@event == EVENTCODE.BEACONACTIVATE) hasAdded = true;
            if (@event == EVENTCODE.NESTSPILL) hasSpilled = true;
            if (@event == EVENTCODE.NESTKICK) hasClicked = true;
        }
    }

}