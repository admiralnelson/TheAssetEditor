// T-he following ifdef block is the standard way of creating macros which make exporting
// from a DLL slogfunc::impler. All files within this DLL are compiled with the FBXWRAPPERDLL_EXPORTS
// symbol defined on the command line. This symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see
// FBXWRAPPERDLL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#pragma once

#include <fbxsdk.h>
#include <DirectXMath.h>
#include <d3d.h>
#include <Vector>
#include <memory>
#include <string.h>

#include "..\Base\BaseInteropObject.h"
#include "..\Logging\Logging.h"
#include "..\DataStructures\PackedMeshStructs.h"
#include "..\Helpers\FBXHelperFileUtil.h"
#include "..\Helpers\Geometry\FBXNodeSearcher.h"
#include "..\DataStructures\FileInfoData.h"
#include "..\DLLDefines.h"

namespace wrapdll
{
	class FBXSCeneContainer : public BaseInteropObject
	{
	public:		
		virtual ~FBXSCeneContainer()
		{			
#ifdef _DEBUG
			LogInfo("FBXSCeneContainer destroyed.");
#endif // _DEBUG			
		};

        void GetVertices(int meshindex, ExtPackedCommonVertex** ppVertices, int* itemCount);
        void GetIndices(int meshindex, uint16_t** ppVertices, int* itemCount);
        void GetVertexWeights(int meshindex, VertexWeight** pVertexWeights, int* itemCount);

        void AllocateMeshes(int count);
        void SetVertices(int meshindex, VertexWeight* pVerticess, int vertexCount);
        void SetIndices(int meshindex, VertexWeight* pIndices, int vertexCount);
        void SetVertexWeights(int meshindex, VertexWeight* pVertexWeights, int* weightCOunt);
        
        std::vector <PackedMesh>& GetMeshes()
		{
			return m_packedMeshes;
		};

		std::string& GetSkeletonName()
		{
            return m_skeletonName;
		};        	

		FbxFileInfoData& GetFileInfo()
		{            
            // TODO: file out rest and display in "GileInfoView"

			return m_fileInfoStruct;
		};

	private:        
        FbxFileInfoData m_fileInfoStruct;
          
        // TODO: remove?
		std::string m_skeletonName = "";
		std::vector<PackedMesh> m_packedMeshes;		
		std::vector<std::string> m_animFileBoneNames; // ordered as the .ANIM file, so can be used for bonename -> index lookups
		fbxsdk::FbxScene* m_pFbxScene = nullptr;
	};
}