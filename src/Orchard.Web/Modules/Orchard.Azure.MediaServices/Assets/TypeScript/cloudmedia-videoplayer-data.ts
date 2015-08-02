module Orchard.Azure.MediaServices.VideoPlayer.Data {

    export interface IAssetData {
        VideoAssets: IVideoAsset[];
        DynamicVideoAssets: IDynamicVideoAsset[];
        ThumbnailAssets: IThumbnailAsset[];
        SubtitleAssets: ISubtitleAsset[];
    }

    export interface IAsset {
        Type: AssetType;
        Id: number;
        Name: string;
        MimeType: string;
        MainFileUrl: string;
        MediaQuery: string;
    }

    export enum AssetType {
        VideoAsset,
        DynamicVideoAsset,
        ThumbnailAsset,
        SubtitleAsset
    }

    export interface IVideoAsset extends IAsset {
        EncodingPreset: string;
        EncoderMetadata: IEncoderMetadata;
    }

    export interface IDynamicVideoAsset extends IVideoAsset {
        SmoothStreamingUrl: string;
        HlsUrl: string;
        MpegDashUrl: string;
    }

    export interface IThumbnailAsset extends IAsset {
    }

    export interface ISubtitleAsset extends IAsset {
        Language: string;
    }

    export interface IEncoderMetadata {
        AssetFiles: IAssetFile[];
    }

    export interface IAssetFile {
        Name: string;
        Size: number;
        Duration: Duration;
        AudioTracks: IAudioTrack[];
        VideoTracks: IVideoTrack[];
        Sources: string[];
        Bitrate: number;
        MimeType: string;
    }

    export interface IAudioTrack {
        Index: number;
        Bitrate: number;
        SamplingRate: number;
        BitsPerSample: number;
        Channels: number;
        Codec: string;
        EncoderVersion: string;
    }

    export interface IVideoTrack {
        Index: number;
        Bitrate: number;
        TargetBitrate: number;
        Framerate: number;
        TargetFramerate: number;
        FourCc: string;
        Width: number;
        Height: number;
        DisplayRatioX: number;
        DisplayRatioY: number;
    }
} 