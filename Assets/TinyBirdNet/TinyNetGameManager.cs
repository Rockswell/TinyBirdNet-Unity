﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;

namespace TinyBirdNet {

	public class TinyNetGameManager : MonoBehaviour {

		public static TinyNetGameManager instance;

		[SerializeField] List<GameObject> registeredPrefabs;

		[SerializeField] List<string> prefabsGUID;

		public LogFilter currentLogFilter = LogFilter.Info;

		[SerializeField] protected int maxNumberOfPlayers = 4;
		protected int port = 7777;
		protected int pingInterval = 1000;

		public bool bNatPunchEnabled { get; protected set; }

		public int MaxNumberOfPlayers { get { return maxNumberOfPlayers; } }
		public int Port { get { return port; } }
		public int PingInterval {
			get { return pingInterval; }
			set {
				if (serverManager != null) {
					serverManager.SetPingInterval(value);
				}
				if (clientManager != null) {
					clientManager.SetPingInterval(value);
				}

				pingInterval = value;
			}
		}

		protected TinyNetServer serverManager;
		protected TinyNetClient clientManager;

		public bool isServer { get { return serverManager != null && serverManager.isRunning; } }
		public bool isClient { get { return clientManager != null && clientManager.isRunning; } }
		public bool isListenServer { get { return isServer && isClient; } }
		//public bool isStandalone { get; protected set; }

		private int _nextNetworkID = 0;
		public int NextNetworkID {
			get {
				return ++_nextNetworkID;
			}
		}

		void Awake() {
			instance = this;

			TinyNetLogLevel.currentLevel = currentLogFilter;

			TinyNetReflector.GetAllSyncVarProps();

			//serverManager = new TinyNetServer();
			//clientManager = new TinyNetClient();

			AwakeVirtual();
		}

		/** <summary>Please override this function to use the Awake call.</summary> */
		protected virtual void AwakeVirtual() { }

		void Start() {
			StartVirtual();
		}

		/** <summary>Please override this function to use the Start call.</summary> */
		protected virtual void StartVirtual() { }

		void Update() {
			if (serverManager != null) {
				serverManager.InternalUpdate();
			}
			if (clientManager != null) {
				clientManager.InternalUpdate();
			}

			UpdateVirtual();
		}

		/** <summary>Please override this function to use the Update call.</summary> */
		protected virtual void UpdateVirtual() { }

		void OnDestroy() {
			ClearNetManager();
		}

		public int GetAssetIdFromPrefab(GameObject prefab) {
			return registeredPrefabs.IndexOf(prefab);
		}

		public int GetAssetIdFromAssetGUID(string assetGUID) {
			return prefabsGUID.IndexOf(assetGUID);
		}

		public GameObject GetPrefabFromAssetId(int assetId) {
			return registeredPrefabs[assetId];
		}

		public GameObject GetPrefabFromAssetGUID(string assetGUID) {
			return registeredPrefabs[prefabsGUID.IndexOf(assetGUID)];
		}

		public int GetAmountOfRegisteredAssets() {
			return registeredPrefabs.Count;
		}

		void MakeListOfPrefabsGUID() {
			prefabsGUID = new List<string>(registeredPrefabs.Count);

			for (int i = 0; i < registeredPrefabs.Count; i++) {
				prefabsGUID.Add(registeredPrefabs[i].GetComponent<TinyNetIdentity>().assetGUID);
			}
		}

		/*public Dictionary<string, GameObject> GetDictionaryOfAssetGUIDToPrefabs() {
			Dictionary<string, GameObject> result = new Dictionary<string, GameObject>(registeredPrefabs.Count);

			for (int i = 0; i < registeredPrefabs.Count; i++) {
				result.Add(registeredPrefabs[i].GetComponent<TinyNetIdentity>().assetGUID, registeredPrefabs[i]);
			}

			return result;
		}*/

		/// <summary>
		/// Receives a new list of registered prefabs from the custom Editor.
		/// </summary>
		public void RebuildAllRegisteredPrefabs(GameObject[] newArray) {
			registeredPrefabs = new List<GameObject>(newArray);
		}

		protected virtual void ClearNetManager() {
			if (serverManager != null) {
				serverManager.ClearNetManager();
			}

			if (clientManager != null) {
				clientManager.ClearNetManager();
			}
		}

		/** <summary>Changes the current max amount of players, this only has an effect before starting a Server.</summary> */
		public virtual void SetMaxNumberOfPlayers(int newNumber) {
			if (serverManager != null) {
				return;
			}
			maxNumberOfPlayers = newNumber;
		}

		/** <summary>Changes the port that will be used for hosting, this only has an effect before starting a Server.</summary> */
		public virtual void SetPort(int newPort) {
			if (serverManager != null) {
				return;
			}
			port = newPort;
		}

		public virtual void ToggleNatPunching(bool bNewState) {
			bNatPunchEnabled = bNewState;
		}

		/// <summary>
		/// Prepares this game to work as a server.
		/// </summary>
		public virtual void StartServer() {
			serverManager = new TinyNetServer();

			serverManager.StartServer(port, maxNumberOfPlayers);
		}

		/// <summary>
		/// Prepares this game to work as a client.
		/// </summary>
		public virtual void StartClient() {
			clientManager = new TinyNetClient();

			clientManager.StartClient();
		}

		/// <summary>
		/// Attempts to connect to the target server, StartClient() must have been called before.
		/// </summary>
		/// <param name="hostAddress">An IPv4 or IPv6 string containing the address of the server.</param>
		/// <param name="hostPort">An int representing the port to use for the connection.</param>
		public virtual void ClientConnectTo(string hostAddress, int hostPort) {
			clientManager.ClientConnectTo(hostAddress, hostPort);
		}
	}
}