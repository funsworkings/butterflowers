using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XNode.Examples.MathNodes;

using UnityEngine.Events;

namespace Intro {

    public class DesktopVisualization: MonoBehaviour {

        public UnityEvent onComplete;
        public TextureCollection StarterPack;

        RectTransform area;

        [Header("External")]
            [SerializeField] Quilt quilt;
            [SerializeField] MotherOfButterflies mother;

        [Header("Spawn attributes")]
            [SerializeField] GameObject pr_desktopFile;
            [SerializeField] RectTransform desktopRoot;
            [SerializeField] float maxTimeBetweenSpawn = 1f;
            [SerializeField] List<GameObject> files = new List<GameObject>();
            Dictionary<GameObject, Texture2D> file_lookup = new Dictionary<GameObject, Texture2D>();

        [Header("Absorb attributes")]
            [SerializeField] float timeToAbsorbFiles = 1f;

        void Awake() {
            area = GetComponent<RectTransform>();
        }


        /* * * * * * * * * * * * * * * * */


        public void SpawnDesktopFiles()
        {
            var files = StarterPack.elements;
            var area = this.area.rect;

            StartCoroutine(SpawningDesktopFiles(files, area));
        }

        IEnumerator SpawningDesktopFiles(Texture2D[] files, Rect area)
        {
            for (int i = 0; i < files.Length; i++) {
                var x = Random.Range(-area.width / 2f, area.width / 2f);
                var y = Random.Range(-area.height / 2f, area.height / 2f);

                var file = Instantiate(pr_desktopFile, transform);

                var file_rect = file.GetComponent<RectTransform>();
                var file_img = file.GetComponent<RawImage>();

                file_rect.anchoredPosition = new Vector2(x, y);
                file_img.texture = files[i];

                this.files.Add(file);
                file_lookup.Add(file, files[i]);

                yield return new WaitForSeconds(Random.Range(0f, maxTimeBetweenSpawn));
            }
        }

        public void AbsorbDesktopFiles()
        {
            StartCoroutine(AbsorbingDesktopFiles());
        }

        IEnumerator AbsorbingDesktopFiles()
        {
            float t = 0f;
            float len = 0f;
            float interval = 0f;

            Vector2 target = desktopRoot.position;
            
            for (int i = 0; i < files.Count; i++) {
                var file = files[i];
                var transform = file.GetComponent<RectTransform>();

                len = Random.Range(timeToAbsorbFiles / 2f, timeToAbsorbFiles);

                while (t < timeToAbsorbFiles) {
                    t += Time.deltaTime;
                    interval = Mathf.Clamp01(t / timeToAbsorbFiles);

                    var amt = Mathf.Pow(interval, 2f);

                    transform.position = Vector2.Lerp(transform.position, target, amt);
                    transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, amt);

                    yield return null;
                }

                quilt.Add(file_lookup[file]);
                t = 0f;
            }
        }


        public void KillAllButterflies()
        {
            mother.KillButterflies(false);
        }

        public void WaitForComplete()
        {
            StartCoroutine(WaitForButterflyDeath());
        }

        IEnumerator WaitForButterflyDeath()
        {
            yield return new WaitForSeconds(3.33f);
            onComplete.Invoke();
        }
    }

}