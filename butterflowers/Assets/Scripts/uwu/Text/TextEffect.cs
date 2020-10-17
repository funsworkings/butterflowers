using System.Collections;
using TMPro;
using UnityEngine;

namespace uwu.Text
{
	[RequireComponent(typeof(TMP_Text))]
	public abstract class TextEffect : MonoBehaviour
	{
		bool hasTextChanged;
		TMP_Text m_TextComponent;

		#region TMP callbacks

		void ON_TEXT_CHANGED(Object obj)
		{
			if (obj == m_TextComponent)
				hasTextChanged = true;
		}

		#endregion


		IEnumerator AnimateVertexColors()
		{
			// We force an update of the text object since it would only be updated at the end of the frame. Ie. before this code is executed on the first frame.
			// Alternatively, we could yield and wait until the end of the frame when the text object will be generated.
			m_TextComponent.ForceMeshUpdate();

			var textInfo = m_TextComponent.textInfo;

			Matrix4x4 matrix;

			var loopCount = 0;
			hasTextChanged = true;

			// Cache the vertex data of the text object as the Jitter FX is applied to the original position of the characters.
			var cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

			while (true) {
				// Get new copy of vertex data if the text has changed.
				if (hasTextChanged) {
					// Update the copy of the vertex data for the text object.
					cachedMeshInfo = textInfo.CopyMeshInfoVertexData();
					hasTextChanged = false;
				}

				var characterCount = textInfo.characterCount;

				// If No Characters then just yield and wait for some text to be added
				if (characterCount == 0) {
					yield return new WaitForSeconds(0.25f);
					continue;
				}

				var move_offset = Vector3.zero;
				var rot_offset = Vector3.zero;
				var scale_offset = Vector3.one;

				for (var i = 0; i < characterCount; i++) {
					var charInfo = textInfo.characterInfo[i];

					// Skip characters that are not visible and thus have no geometry to manipulate.
					if (!charInfo.isVisible)
						continue;

					// Get the index of the material used by the current character.
					var materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

					// Get the index of the first vertex used by this text element.
					var vertexIndex = textInfo.characterInfo[i].vertexIndex;

					// Get the cached vertices of the mesh used by this text element (character or sprite).
					var sourceVertices = cachedMeshInfo[materialIndex].vertices;

					// Determine the center point of each character at the baseline.
					//Vector2 charMidBasline = new Vector2((sourceVertices[vertexIndex + 0].x + sourceVertices[vertexIndex + 2].x) / 2, charInfo.baseLine);
					// Determine the center point of each character.
					Vector2 charMidBasline = (sourceVertices[vertexIndex + 0] + sourceVertices[vertexIndex + 2]) / 2;

					// Need to translate all 4 vertices of each quad to aligned with middle of character / baseline.
					// This is needed so the matrix TRS is applied at the origin for each character.
					Vector3 offset = charMidBasline;

					var destinationVertices = textInfo.meshInfo[materialIndex].vertices;

					destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] - offset;
					destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] - offset;
					destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] - offset;
					destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] - offset;


					/* * * * * * * * * * * * * * * * * * * * * * * * */

					ComputeTranslationPerVertex(ref move_offset, i, characterCount);
					ComputeRotationPerVertex(ref rot_offset, i, characterCount);
					ComputeScalePerVertex(ref scale_offset, i, characterCount);

					/* * * * * * * * * * * * * * * * * * * * * * * * */


					matrix = Matrix4x4.TRS(move_offset, Quaternion.Euler(rot_offset), scale_offset);

					destinationVertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 0]);
					destinationVertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 1]);
					destinationVertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 2]);
					destinationVertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(destinationVertices[vertexIndex + 3]);

					destinationVertices[vertexIndex + 0] += offset;
					destinationVertices[vertexIndex + 1] += offset;
					destinationVertices[vertexIndex + 2] += offset;
					destinationVertices[vertexIndex + 3] += offset;
				}

				// Push changes into meshes
				for (var i = 0; i < textInfo.meshInfo.Length; i++) {
					textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
					m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
				}

				loopCount += 1;

				yield return new WaitForSeconds(0.1f);
			}
		}

		protected virtual void ComputeTranslationPerVertex(ref Vector3 offset, int index, int length)
		{
			offset = Vector3.zero;
		}

		protected virtual void ComputeRotationPerVertex(ref Vector3 offset, int index, int length)
		{
			offset = Vector3.zero;
		}

		protected virtual void ComputeScalePerVertex(ref Vector3 offset, int index, int length)
		{
			offset = Vector3.one;
		}

		#region Monobehaviour callbacks

		void Awake()
		{
			m_TextComponent = GetComponent<TMP_Text>();
		}

		void OnEnable()
		{
			// Subscribe to event fired when text object has been regenerated.
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(ON_TEXT_CHANGED);
			StartCoroutine(AnimateVertexColors());
		}

		void OnDisable()
		{
			TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(ON_TEXT_CHANGED);
			StopCoroutine(AnimateVertexColors());
		}

		#endregion
	}
}