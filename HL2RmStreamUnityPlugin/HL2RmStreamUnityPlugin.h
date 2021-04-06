#pragma once

#ifdef FUNCTIONS_EXPORTS  
#define FUNCTIONS_EXPORTS_API extern "C" __declspec(dllexport)   
#else  
#define FUNCTIONS_EXPORTS_API extern "C" __declspec(dllimport)   
#endif  

namespace HL2Stream
{
	FUNCTIONS_EXPORTS_API void __stdcall StartStreaming();

	FUNCTIONS_EXPORTS_API void StreamingToggle();

	winrt::Windows::Foundation::IAsyncAction
		InitializeVideoFrameProcessorAsync();

	winrt::Windows::Perception::Spatial::SpatialCoordinateSystem
		m_worldOrigin{ nullptr };
	std::wstring m_patient;

	std::unique_ptr<VideoCameraStreamer> m_videoFrameProcessor = nullptr;
	winrt::Windows::Foundation::IAsyncAction m_videoFrameProcessorOperation = nullptr;
}