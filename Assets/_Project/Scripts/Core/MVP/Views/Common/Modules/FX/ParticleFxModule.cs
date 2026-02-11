using NaughtyAttributes;
using UnityEngine;

namespace Core.Modules
{
    public class ParticleFxModule : MonoBehaviour
    {
        [field: SerializeField, Required] public ParticleSystem ParticleSystem { get; private set; }
        
        public void Emit(int count) => ParticleSystem.Emit(count);
        public bool IsPlaying => ParticleSystem&& ParticleSystem.isPlaying;
        public bool IsAlive => ParticleSystem&& ParticleSystem.IsAlive(true);
        public int ParticleCount => ParticleSystem.particleCount;
        
        public void Play() => ParticleSystem.Play();
        
        public void Stop(bool clear = false)
        {
            ParticleSystem.Stop(true, clear ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }
        
        public void Pause() => ParticleSystem.Pause();
        public void Clear() => ParticleSystem.Clear();
        
        public void Restart()
        {
            ParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.Play();
        }
        
        public void SetEmissionRate(float rate)
        {
            var emission = ParticleSystem.emission;
            emission.rateOverTime = rate;
        }
        
        public void SetStartColor(Color color)
        {
            var main = ParticleSystem.main;
            main.startColor = color;
        }
        
        public void SetStartSize(float size)
        {
            var main = ParticleSystem.main;
            main.startSize = size;
        }
        
        public void SetStartSpeed(float speed)
        {
            var main = ParticleSystem.main;
            main.startSpeed = speed;
        }
        
        public void SetStartLifetime(float lifetime)
        {
            var main = ParticleSystem.main;
            main.startLifetime = lifetime;
        }
        
        public void SetDuration(float duration)
        {
            var main = ParticleSystem.main;
            main.duration = duration;
        }
        
        public void SetGravityModifier(float gravity)
        {
            var main = ParticleSystem.main;
            main.gravityModifier = gravity;
        }
        
        public void SetSimulationSpeed(float speed)
        {
            var main = ParticleSystem.main;
            main.simulationSpeed = speed;
        }
        
        public void SetMaxParticles(int max)
        {
            var main = ParticleSystem.main;
            main.maxParticles = max;
        }
        
        public void SetTexture(Texture texture)
        {
            var renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
            
            if (renderer)
            {
                var material = renderer.material;
                material.mainTexture = texture;
            }
        }
        
        public void SetMaterial(Material material)
        {
            var renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
            
            if (renderer)
            {
                renderer.material = material;
            }
        }
        
        public void SetTextureSheetSprites(Sprite[] sprites)
        {
            var textureSheet = ParticleSystem.textureSheetAnimation;
            textureSheet.enabled = true;
            textureSheet.mode = ParticleSystemAnimationMode.Sprites;
            
            for (int i = textureSheet.spriteCount - 1; i >= 0; i--)
            {
                textureSheet.RemoveSprite(i);
            }
            
            foreach (var sprite in sprites)
            {
                textureSheet.AddSprite(sprite);
            }
        }
    }
}