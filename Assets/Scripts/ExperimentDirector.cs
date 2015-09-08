using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class ExperimentDirector : MonoBehaviour {

	public enum ColliderMode { Box,  Sphere, Max }
	public enum CastMode     { Line, Sphere, Max }
	public enum ButtonType   { ColliderMode, CastMode, Cast, Clear, Increment, IncMany, Max }

	[System.Serializable]
	public class ColSet {
		public Transform  caster;
		public Transform  colGroup;
		public GameObject colObject;
	}
	public List<ColSet> colSet;

	public Text infoText;

	private List<System.Action> btnFuncs;

	public class CurInfo {
		public ColSet       colSet;
		public ColliderMode colMode;
		public CastMode     castMode;
		public int[]        colNums  = new int[(int)ColliderMode.Max];
		public bool         isHit    = false;
		public int          castTime = 0;

		public override string ToString() {
			return "Cols: " + ArrayToString(colNums)  + "\n"
				+  "Cast: " + castMode + "\n"
				+  "Col:  " + colMode  + "\n"
				+  "Hit:  " + isHit    + "\n"
				+  "Time: " + castTime;
		}


		private static string ArrayToString(int[] array) {
			string str = "[" + (array.Count () == 0 ? "" : array[0].ToString()); 
			for (int i = 1; i < array.Count (); i++) {
				str += ", " + array [i].ToString();
			}
			str += "]";
			return str;
		}

	}
	private CurInfo cur;

	private const int CAST_NUMS = 10000;

	private void Start() {
		Initialize ();
	}

	public void Initialize() {
		btnFuncs = new List<System.Action> () {
			SwtichColliderMode,
			SwitchCastMode,
			TestCast,
			ClearCols,
			AddCol,
			AddManyCol
		};

		cur = new CurInfo();
		cur.colSet   = colSet [0];
		cur.colMode  = ColliderMode.Box;
		cur.castMode = CastMode.Line;

		UpdateInfoText ();
	}
	private void UpdateInfoText() {
		infoText.text = "[" + System.DateTime.Now.ToString () + "]\n" + cur.ToString ();
	}

	public void OnButtonClicked(int type) {
		btnFuncs [type] ();

		UpdateInfoText ();
	}

	public void SwtichColliderMode() {
		cur.colMode = (ColliderMode)(((int)cur.colMode + 1) % (int)ColliderMode.Max);
	}
	public void SwitchCastMode() {
		cur.castMode = (CastMode)(((int)cur.castMode + 1) % (int)CastMode.Max);
	}
	public void TestCast() {
		System.DateTime start = System.DateTime.Now;

		foreach (int i in Enumerable.Range(0, CAST_NUMS)) {
			cur.isHit = Cast ();
		}

		System.DateTime end   = System.DateTime.Now;
		cur.castTime = (end - start).Milliseconds;
	}
	public void ClearCols() {
		ColSet curSet = colSet [(int)cur.colMode];
		cur.colNums [(int)cur.colMode] = 0;

		curSet.colGroup
			.GetComponentsInChildren<Transform> ()
			.Where (t => t != curSet.colGroup)
			.ToList ().ForEach (t => Destroy(t.gameObject));
	}
	public void AddCol() {
		ColSet curSet = colSet [(int)cur.colMode];
		cur.colNums [(int)cur.colMode]++;

		GameObject obj = Instantiate (curSet.colObject);
		obj.transform.SetParent (curSet.colGroup);
		obj.transform.localPosition = Vector3.zero;

		int childNums = curSet.colGroup.GetComponentsInChildren<Transform> ().Count() - 1;
		if (childNums > 1) {
			obj.transform.localPosition = new Vector3 (
				Random.value * 10f - 5, 0f, Random.value * 10f - 5
			);
		}

	}
	public void AddManyCol() {
		foreach (int i in Enumerable.Range(0, 10)) {
			AddCol ();
		}
	}

	private bool Cast() {
		ColSet curSet = colSet [(int)cur.colMode];
		RaycastHit hit;

		if (cur.castMode == CastMode.Line) {
			return Physics.Raycast (curSet.caster.position, curSet.caster.forward, out hit, 20f);
		} else if (cur.castMode == CastMode.Sphere) {
			return Physics.SphereCast (curSet.caster.position, 1f, curSet.caster.forward, out hit, 20f);
		}

		return false;
	}

}
