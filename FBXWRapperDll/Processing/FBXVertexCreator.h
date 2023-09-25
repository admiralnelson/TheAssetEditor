#pragma once

#include "..\DataStructures\PackedMeshStructs.h"
#include "..\Helpers\FBXUnitHelper.h"

namespace wrapdll{

	class FBXVertexhCreator
	{
	public:
		/// <summary>
		/// Converts FBX data structure to the common (packed) vertex format
		/// </summary>
		/// <param name="vControlPoint">Vertex Position</param>
		/// <param name="vNormalVector">Normal Vector</param>
		/// <param name="UVmap1">UV coords</param>
		/// <param name="ctrlPointInfluences">Data from the FBXSkin processing</param>
		/// <param name="scaleFactor"></param>
		/// <returns></returns>
		static ExtPackedCommonVertex MakePackedVertex(
			const FbxVector4& vControlPoint,
			const FbxVector4& vNormalVector,
			const FbxVector2& UVmap1,
			const ControlPointInfluence* ctrlPointInfluences,
			double scaleFactor)
		{
			ExtPackedCommonVertex outVertex;

			outVertex.position.x = static_cast<float>(vControlPoint.mData[0] * scaleFactor);
			outVertex.position.y = static_cast<float>(vControlPoint.mData[1] * scaleFactor);
			outVertex.position.z = static_cast<float>(vControlPoint.mData[2] * scaleFactor);

			outVertex.normal.x = static_cast<float>(vNormalVector.mData[0]);
			outVertex.normal.y = static_cast<float>(vNormalVector.mData[1]);
			outVertex.normal.z = static_cast<float>(vNormalVector.mData[2]);

			outVertex.uv.x = static_cast<float>(UVmap1.mData[0]);
			outVertex.uv.y = static_cast <float>(UVmap1.mData[1]);        

			return outVertex;
        }
    
    private:
        // TODO: remove?
        
        //static void CopyVertexWeighting(const ControlPointInfluence* ctrlPointInfluences, ExtPackedCommonVertex& outVertex)
        //{
        //    if (ctrlPointInfluences)
        //    {
        //        outVertex.weightCount = ctrlPointInfluences->weightCount;

        //        for (size_t i = 0; i < outVertex.weightCount; i++)
        //        {
        //            outVertex.influences[i] = ctrlPointInfluences->influences[i].Clone();

        //            // TODO: neede?
        //            //strcpy_s<255>(outVertex.influences[i].boneName, ctrlPointInfluences->influences[i].boneName.c_str());
        //            //outVertex.influences[i].boneIndex = ctrlPointInfluences->influences[i].boneIndex;
        //            //outVertex.influences[i].weight = ctrlPointInfluences->influences[i].weight;
        //        }
        //    }
        //    else
        //        outVertex.weightCount = 0;
        //};
	};
}