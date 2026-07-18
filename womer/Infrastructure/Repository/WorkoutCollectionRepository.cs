using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using womer.Core.Interfaces;
using womer.Core.Models;
using womer.Infrastructure.Settings;

namespace womer.Infrastructure.Repository
{
    public class WorkoutCollectionRepository : IWorkoutCollectionRepository
    {
        private readonly IPreferences _preferences;
        private readonly SemaphoreSlim _lock = new(1, 1);
        public WorkoutCollectionRepository(IPreferences preferences)
        {
            if (preferences == null)    throw new ArgumentNullException(nameof(preferences));
            _preferences = preferences;
        }
        public async Task DeleteByIdAsync(long id)
        {
            await _lock.WaitAsync();
            try
            {
                var list = LoadList();
                var itemToRemove = list.FirstOrDefault(x => x.Id == id);
                if (itemToRemove != null)
                {
                    list.Remove(itemToRemove);
                    PersistList(list);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while deleting the workout collection with ID {id}.", ex);
            }
            finally
            {
                _lock.Release();

            }
        }

        public async Task<IEnumerable<WorkoutCollection>> GetAllAsync()
        {
            await _lock.WaitAsync();
            try
            {
                var list = LoadList();
                return list;

            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while retrieving the workout collections.", ex);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<WorkoutCollection?> GetByIdAsync(long id)
        {
            await _lock.WaitAsync();
            try
            {
                var list = LoadList();
                return list.FirstOrDefault(x => x.Id == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving the workout collection with ID {id}.", ex);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<WorkoutCollection> SaveAsync(WorkoutCollection workoutCollection)
        {
            await _lock.WaitAsync();
            try
            {
                var list = LoadList();
                workoutCollection.Id = list.Count > 0 ? list.Max(x => x.Id) + 1 : 1;
                list.Add(workoutCollection);
                PersistList(list);
                return workoutCollection;

            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while saving the workout collection.", ex);
            }
            finally
            {
                _lock.Release();
            }
            
        }

        private List<WorkoutCollection> LoadList()
        {
            var json = _preferences.Get<string>(StorageKeys.WorkoutCollection.List, null);

            if (string.IsNullOrEmpty(json))
                return new List<WorkoutCollection>();

            return JsonSerializer.Deserialize<List<WorkoutCollection>>(json) ?? new List<WorkoutCollection>();
        }

        private void PersistList(List<WorkoutCollection> list)
        {
            _preferences.Set(StorageKeys.WorkoutCollection.List, JsonSerializer.Serialize(list));
        }

        public async Task<WorkoutCollection> UpdateAsync(WorkoutCollection workoutCollection)
        {
            if (workoutCollection == null)
                throw new ArgumentNullException(nameof(workoutCollection));

            await _lock.WaitAsync();
            try
            {
                var list = LoadList();
                var index = list.FindIndex(x => x.Id == workoutCollection.Id);

                if (index < 0)
                    throw new KeyNotFoundException($"Workout collection with ID {workoutCollection.Id} was not found.");

                list[index] = workoutCollection;
                PersistList(list);

                return workoutCollection;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating the workout collection with ID {workoutCollection.Id}.", ex);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
