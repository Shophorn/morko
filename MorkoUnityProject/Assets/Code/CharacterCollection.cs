// using System.Linq;
// using System.Collections;
// using UnityEngine;

// [CreateAssetMenu]
// public class CharacterCollection : ScriptableObject
// {
//     [SerializeField] private Character [] _characters;

//     public int Count => _characters.Length;

//     public Character InstantiateOne(int characterModelIndex)
//     {
//         return Instantiate(_characters[characterModelIndex]);
//     }

//     public Character [] InstantiateMany(int [] characterModelIds)
//     {
//         int instantiatedCount   = characterModelIds.Length;
//         Character [] characters = new Character [instantiatedCount];
//         for (int i = 0; i < instantiatedCount; i++)
//         {
//             characters[i] = InstantiateOne(characterModelIds[i]);
//         }
//         return characters;
//     }

//     public Character [] InstantiateAll()
//     {
//         return InstantiateMany(Enumerable.Range(0, Count).ToArray());
//     }
// }