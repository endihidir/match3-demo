namespace Core.UI
{
    public abstract class BaseParticleFXView : BaseFxView
    {
        public abstract void Play();
        public abstract void Stop(bool clear);
    }
}