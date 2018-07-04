using Kino;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteOblivion
{
    public class TriggerEvent_Player : MonoBehaviour {

        public string triggerType;
        public Camera camera;
        public Object endingParticles;

        private Bloom mainBloom;
        private float targetBloom = .4f;
        private float curBloom;
        private float bloomSmoothing = 0.0f;
        private float smoothTime = 1.2f;

        private void Awake()
        {
            mainBloom = camera.GetComponent<Bloom>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(collision.gameObject.GetComponent<PBehaviour>() is PBehaviour)
            {
                if (triggerType.Equals("Stop Player"))
                {
                    PScytheMovement player = collision.gameObject.GetComponent<PScytheMovement>();
                    PState Waiting = new PState();
                    player.Transition(Waiting);
                    player.velocity = Vector2.zero;
                    StartCoroutine(WaitForParticleSpawn());
                    
                }
                else if(triggerType.Equals("Start Bloom"))
                {
                    StartCoroutine(DoBloomSmoothing());
                    //mainBloom.softKnee = .4f;
                }
            }
        }

        IEnumerator DoBloomSmoothing()
        {
            while (curBloom < targetBloom)
            {
                curBloom = Mathf.SmoothDamp(curBloom, targetBloom, ref bloomSmoothing, smoothTime);
                mainBloom.softKnee = curBloom;
                yield return new WaitForSeconds(.05f);
            }
        }

        IEnumerator WaitForParticleSpawn()
        {
            yield return new WaitForSeconds(3f);
            Instantiate(endingParticles);
        }
    }
}
