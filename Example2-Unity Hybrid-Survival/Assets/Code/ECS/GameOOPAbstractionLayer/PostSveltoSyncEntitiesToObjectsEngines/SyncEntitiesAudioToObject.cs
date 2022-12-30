namespace Svelto.ECS.Example.Survive.OOPLayer
{
    public class SyncEntitiesAudioToObject: IQueryingEntitiesEngine, IStepEngine
    {
        public SyncEntitiesAudioToObject(GameObjectResourceManager manager)
        {
            _manager = manager;
        }

        public void Ready() { }

        public EntitiesDB entitiesDB { get; set; }
        public void Step()
        {
            var groups = entitiesDB.FindGroups<GameObjectEntityComponent, SoundComponent>();
            //animation sync
            foreach (var ((entity, audios, count), _) in entitiesDB
                            .QueryEntities<GameObjectEntityComponent, SoundComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    if (audios[i].playOneShot != 0)
                    {
                        var go = _manager[entity[i].resourceIndex];
                        var audio = go.GetComponent<AudioImplementor>();
                        
                        audio.PlayOneShot(audios[i].playOneShot);

                        audios[i].playOneShot = 0;
                    }
                }
            }
        }

        public string name => nameof(SyncEntitiesAudioToObject);

        readonly GameObjectResourceManager _manager;
    }
}