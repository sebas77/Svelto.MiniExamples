namespace Svelto.ECS.Example.Survive.Characters.Enemies
{
    public interface IEnemyFactory
    {
        void Build(EnemySpawnData spawnDataEnemySpawnData, ref EnemyAttackStruct enemyAttackstruct);
        void Preallocate();
    }
}