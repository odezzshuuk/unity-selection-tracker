using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Odezzshuuk.Editor.SelectionTracker {

  /// <summary>
  /// Manages persistence and coordination of various entry tracking services in the Selection Tracker system.
  /// This singleton class maintains different service types (History, Favorites, Most Visited, Scene Components)
  /// and provides a centralized interface for recording and retrieving selection entries.
  /// Data is automatically saved to "UserSettings/SelectionTracker.asset" in the project folder.
  /// </summary>
  [FilePath("UserSettings/SelectionTracker.asset", FilePathAttribute.Location.ProjectFolder)]
  public class EntryServicePersistence : ScriptableSingleton<EntryServicePersistence> {

    [SerializeReference]
    private List<IEntryService> _entryServices;

    private HistoryService _historyService;
    private MostVisitedService _mostVisitedService;
    private FavoritesService _favoritesService;
    private SceneComponentsService _sceneComponentsService;

    /// <summary>
    /// Called when the ScriptableSingleton is loaded. Registers update listeners for all entry services
    /// to ensure changes are automatically saved to disk.
    /// </summary>
    public void OnEnable() {
      // initialize services list
      if (_entryServices == null) {
        _entryServices = new();
      } else {
        _entryServices.RemoveAll(s => s == null);
      }

      _historyService = GetService<HistoryService>();
      _historyService.OnUpdated.RemoveListener(OnServiceUpdate);
      _historyService.OnUpdated.AddListener(OnServiceUpdate);

      _mostVisitedService = GetService<MostVisitedService>();
      _mostVisitedService.OnUpdated.RemoveListener(OnServiceUpdate);
      _mostVisitedService.OnUpdated.AddListener(OnServiceUpdate);

      _favoritesService = GetService<FavoritesService>();
      _favoritesService.OnUpdated.RemoveListener(OnServiceUpdate);
      _favoritesService.OnUpdated.AddListener(OnServiceUpdate);

      _sceneComponentsService = GetService<SceneComponentsService>();
      _sceneComponentsService.OnUpdated.RemoveListener(OnServiceUpdate);
      _sceneComponentsService.OnUpdated.AddListener(OnServiceUpdate);

      // foreach (IEntryService entryService in _entryServices) {
      //   entryService?.OnUpdated.AddListener(OnServiceUpdate);
      // }
    }

    /// <summary>
    /// Retrieves a persist service of the specified type.
    /// If the service doesn't exist, it creates a new instance and adds it to the service list.
    /// </summary>
    /// <typeparam name="T">The type of entry service to retrieve, must implement IEntryService.</typeparam>
    /// <returns>The requested entry service instance.</returns>
    public T GetService<T>() where T : IEntryService, new() {
      IEntryService service = _entryServices.FirstOrDefault(static s => s.GetType() == typeof(T));
      if (service != null) {
        return (T)service;
      }
      T newService = new();
      _entryServices.Add(newService);
      Save();
      return newService;
    }

    /// <summary>
    /// Records a selection entry to both the history and most visited services,
    /// then saves the data to disk.
    /// </summary>
    /// <param name="selection">The entry representing the selected object.</param>
    public void RecordSelection(Entry selection) {
      _historyService?.RecordEntry(selection);
      _mostVisitedService?.RecordEntry(selection);
      Save(true);
    }

    /// <summary>
    /// Records a component entry to the scene components service.
    /// Used for tracking components found in the active scene.
    /// </summary>
    /// <param name="entry">The entry representing a component.</param>
    public void RecordComponent(Entry entry) {
      _sceneComponentsService?.RecordEntry(entry);
    }

    /// <summary>
    /// Records an entry to the favorites service with the specified favorite status,
    /// then saves the data to disk.
    /// </summary>
    /// <param name="entry">The entry to add to favorites.</param>
    /// <param name="isFavorite">Whether the entry should be marked as a favorite (default: false).</param>
    public void RecordFavorites(Entry entry, bool isFavorite = false) {
      GetService<FavoritesService>()?.RecordEntry(entry, isFavorite);
      Save(true);
    }

    /// <summary>
    /// Removes an entry from the favorites service and saves the data to disk.
    /// </summary>
    /// <param name="entry">The entry to remove from favorites.</param>
    public void RemoveFromFavorites(Entry entry) {
      _favoritesService?.RemoveEntry(entry);
      Save(true);
    }

    /// <summary>
    /// Retrieves the previous selection from the history service.
    /// Used for backward navigation through selection history.
    /// </summary>
    /// <returns>The previous entry in the selection history, or null if none exists.</returns>
    public Entry JumpToPreviousSelection() {
      return GetService<HistoryService>()?.PreviousSelection();
    }

    /// <summary>
    /// Retrieves the next selection from the history service.
    /// Used for forward navigation through selection history.
    /// </summary>
    /// <returns>The next entry in the selection history, or null if none exists.</returns>
    public Entry JumpToNextSelection() {
      return GetService<HistoryService>()?.NextSelection();
    }

    /// <summary>
    /// Called when the ScriptableSingleton is being destroyed or disabled.
    /// Unregisters all update listeners from entry services to prevent memory leaks.
    /// </summary>
    public void OnDisable() {
      foreach (IEntryService entryService in _entryServices) {
        entryService?.OnUpdated.RemoveListener(OnServiceUpdate);
      }
    }

    /// <summary>
    /// Manually triggers a save of the persistence data to disk.
    /// This is called automatically after most operations, but can be invoked manually if needed.
    /// </summary>
    public void Save() {
      Save(true);
    }

    /// <summary>
    /// Internal callback triggered when any entry service is updated.
    /// Automatically saves the persistence data to disk to maintain data consistency.
    /// </summary>
    private void OnServiceUpdate() {
      Save();
    }
  }
}

