﻿using System;
using System.Collections.Generic;

using Aragas.Core.Packets;

using MineLib.Core.Data;
using MineLib.Core.Data.Anvil;

namespace MineLib.PGL.Data
{
    public class World : IDisposable
    {
        public List<Chunk> Chunks { get; private set; }

        // -- Debugging
        public static List<string> UnsupportedBlockList = new List<string>();
        public static List<ProtobufPacket> DamagedChunks = new List<ProtobufPacket>();
        // -- Debugging

        //public GameMode GameMode;
        //public Dimension Dimension;
        //public Difficulty Difficulty;
        public GameStateChanged StateChanged;
        public Position SpawnPosition;

        public byte MaxPlayers;

        public string LevelType;

        public long AgeOfTheWorld;
        public long TimeOfDay;

        private const float RealTimeDivisor = 24 * 60 * 60;
        private const float GameHourInRealMinutes = (float)2 / 60;
        private const float GameHourInRealSeconds = GameHourInRealMinutes * 60;
        public float GetGameTimeOfDay()
        {
            return ((TimeOfDay / GameHourInRealSeconds) % 24); // quick demonstration of day & night cycles.
            //return 12; // this disables the day & night cycle.
        }

        public TimeSpan AgeOfTheWorldTimeSpan => TimeSpan.MaxValue;

        public TimeSpan RealTime => new TimeSpan((int)((TimeOfDay / 1000 + 8) % 24), (int)(60 * (TimeOfDay % 1000) / 1000), 0);

        public World()
        {
            Chunks = new List<Chunk>();
        }

        #region Anvil

        public int GetChunkIndex(Coordinates2D coordinates)
        {
            foreach (var chunk in Chunks)
            {
                if (chunk.Coordinates == coordinates)
                    return Chunks.IndexOf(chunk);
            }

            return -1;
        }

        public Chunk GetChunkByBlockCoordinates(Position coordinates)
        {
            if (coordinates.Y < 0 || coordinates.Y >= Chunk.Height)
                throw new ArgumentOutOfRangeException(nameof(coordinates), "Coordinates.Y is out of range");

            var chunkX = coordinates.X >> 4;
            var chunkZ = coordinates.Z >> 4;

             return GetChunk(new Coordinates2D(chunkX, chunkZ));
        }

        public static Coordinates2D ChunkCoordinatesToWorld(Coordinates2D coordinates)
        {
            var chunkX = coordinates.X * Chunk.Width;
            var chunkZ = coordinates.Z * Chunk.Depth;

            return new Coordinates2D(chunkX, chunkZ);
        }

        public Chunk GetChunk(Coordinates2D coordinates)
        {
            foreach (var chunk in Chunks)
                if (chunk.Coordinates == coordinates)
                    return chunk;
            
            return null;
        }
        public void SetChunk(Chunk chunk)
        {
            Chunks.Add(chunk);
        }
        public void RemoveChunk(Coordinates2D coordinates)
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                var chunk = Chunks[i];
                if (chunk.Coordinates == coordinates)
                    Chunks[i] = null;
            }
        }


        public Block GetBlock(Position coordinates)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            var t = chunk.GetBlock(coordinates);
            return t;
        }
        public void SetBlock(Position coordinates, Block block)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            chunk.SetBlock(coordinates, block);
        }
        public void SetBlock(Position coordinates, Coordinates2D chunkCoordinates, Block block)
        {
            var chunk = GetChunk(chunkCoordinates);

            chunk.SetBlockMultiBlock(coordinates, block);
        }

        public byte GetBlockLight(Position coordinates)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            return chunk.GetBlockLight(coordinates);
        }
        public void SetBlockLight(Position coordinates, byte light)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            chunk.SetBlockLight(coordinates, light);
        }

        public byte GetBlockSkylight(Position coordinates)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            return chunk.GetBlockSkylight(coordinates);
        }
        public void SetBlockSkylight(Position coordinates, byte light)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            chunk.SetBlockSkylight(coordinates, light);
        }

        public byte GetBlockBiome(Position coordinates)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            return chunk.GetBlockBiome(coordinates);
        }
        public void SetBlockBiome(Position coordinates, byte biome)
        {
            var chunk = GetChunkByBlockCoordinates(coordinates);

            chunk.SetBlockBiome(coordinates, biome);
        }

        #endregion

        public void Dispose()
        {
            Chunks?.Clear();
        }
    }

    public struct GameStateChanged
    {
        //public GameStateReason Reason;
        public float Value;
    }

}
