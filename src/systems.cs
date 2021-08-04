// ----------------------------------------------------------------------------
// The MIT License
// Lightweight ECS framework https://github.com/Leopotam/ecslite
// Copyright (c) 2021 Leopotam <leopotam@gmail.com>
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if ENABLE_IL2CPP
using Unity.IL2CPP.CompilerServices;
#endif

namespace Leopotam.EcsLite {
    public interface IEcsSystem { }

    public interface IEcsInitSystem : IEcsSystem {
        void Init (EcsWorld world);
    }

    public interface IEcsRunSystem : IEcsSystem {
        void Run (EcsWorld world);
    }

    public interface IEcsDestroySystem : IEcsSystem {
        void Destroy (EcsWorld world);
    }

#if ENABLE_IL2CPP
    [Il2CppSetOption (Option.NullChecks, false)]
    [Il2CppSetOption (Option.ArrayBoundsChecks, false)]
#endif
    public sealed class EcsSystems {
        readonly EcsWorld _defaultWorld;
        readonly List<IEcsSystem> _allSystems;
		readonly List<IEcsRunSystem> _runSystems;

		public EcsSystems (EcsWorld defaultWorld) {
            _defaultWorld = defaultWorld;
            _allSystems = new List<IEcsSystem> (128);
			_runSystems = new List<IEcsRunSystem> (128);
        }

        public int GetAllSystems (ref IEcsSystem[] list) {
            var itemsCount = _allSystems.Count;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new IEcsSystem[itemsCount];
            }
            for (int i = 0, iMax = itemsCount; i < iMax; i++) {
                list[i] = _allSystems[i];
            }
            return itemsCount;
        }

        public int GetRunSystems (ref IEcsRunSystem[] list) {
            var itemsCount = _runSystems.Count;
            if (itemsCount == 0) { return 0; }
            if (list == null || list.Length < itemsCount) {
                list = new IEcsRunSystem[itemsCount];
            }
            for (int i = 0, iMax = itemsCount; i < iMax; i++) {
                list[i] = _runSystems[i];
            }
            return itemsCount;
        }

        [MethodImpl (MethodImplOptions.AggressiveInlining)]
        public EcsWorld GetWorld () {
            return _defaultWorld;
        }

        public void Destroy () {
            for (var i = _allSystems.Count - 1; i >= 0; i--) {
                if (_allSystems[i] is IEcsDestroySystem destroySystem) {
                    destroySystem.Destroy (_defaultWorld);
#if DEBUG
                    if (CheckForLeakedEntities()) { throw new System.Exception ($"Empty entity detected in world \"{_defaultWorld.GetName()}\" after {destroySystem.GetType ().Name}.Destroy()."); }
#endif
                }
            }
            _allSystems.Clear ();
			_runSystems.Clear ();
        }

        public EcsSystems Add (IEcsSystem system) {
            _allSystems.Add (system);
            if (system is IEcsRunSystem runSystem) {
				_runSystems.Add (runSystem);
            }
            return this;
        }

        public void Init () {
			foreach (var system in _allSystems) {
                if (system is IEcsInitSystem initSystem) {
                    initSystem.Init (_defaultWorld);
#if DEBUG
                    if (CheckForLeakedEntities()) { throw new System.Exception ($"Empty entity detected in world \"{_defaultWorld.GetName()}\" after {initSystem.GetType ().Name}.Init()."); }
#endif
                }
            }
        }

        public void Run () {
            for (int i = 0, iMax = _runSystems.Count; i < iMax; i++) {
                _runSystems[i].Run (_defaultWorld);
#if DEBUG
                if (CheckForLeakedEntities()) { throw new System.Exception ($"Empty entity detected in world \"{_defaultWorld.GetName()}\" after {_runSystems[i].GetType ().Name}.Run()."); }
#endif
            }
        }

#if DEBUG
        public bool CheckForLeakedEntities () {
            return _defaultWorld.CheckForLeakedEntities ();
        }
#endif
    }
}
