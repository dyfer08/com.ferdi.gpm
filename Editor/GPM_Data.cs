using UnityEngine;
using System.Collections.Generic;

namespace Ferdi{
[System.Serializable]
public class GPM_Data : ScriptableObject {
	[System.Serializable]
	public class GitPackage {
		public string DisplayName = "";
		public string Repository = "";
	}
	public List<GitPackage> GitPackages = new List<GitPackage>();
}
}
