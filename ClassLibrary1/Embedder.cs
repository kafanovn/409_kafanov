﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using NuGet_ArcFace_Embedder;
using Microsoft.ML.OnnxRuntime;

namespace NuGet_ArcFace_Functions
{
    public class img_comp
    {
        public float Length(float[] v) => (float)Math.Sqrt(v.Select(x => x * x).Sum());
        public float Distance(float[] v1, float[] v2) => Length(v1.Zip(v2).Select(p => p.First - p.Second).ToArray());
        public float Similarity(float[] v1, float[] v2) => v1.Zip(v2).Select(p => p.First * p.Second).Sum();

        public delegate T CalculationCallback<T>(float[] v1, float[] v2);
        public T Execute<T>(Image<Rgb24> img1, Image<Rgb24> img2, CalculationCallback<T> callback)
        {

            float[] embeddings1 = embedder.GetEmbeddings(img1);
            float[] embeddings2 = embedder.GetEmbeddings(img2);
            return callback(embeddings1, embeddings2);

        }

        public async Task<float> ExecuteAsync(float[] embeddings1, float[] embeddings2, CalculationCallback<float> callback, CancellationToken token)
        {
            var res = await Task<float>.Run(async () =>
            {
                Console.WriteLine($"Distance =  {Distance(embeddings1, embeddings2) * Distance(embeddings1, embeddings2)}");
                return callback(embeddings1, embeddings2);
            });
            return res;

        }

        public float Distance(Image<Rgb24> img1, Image<Rgb24> img2)
        { return Execute<float>(img1, img2, Distance); }

        public float Similarity(Image<Rgb24> img1, Image<Rgb24> img2)
        { return Execute<float>(img1, img2, Similarity); }

        public (float distance, float similarity) Distance_and_Similarity(Image<Rgb24> img1, Image<Rgb24> img2)
        { return (Execute<float>(img1, img2, Distance), Execute<float>(img1, img2, Similarity)); }

        public Task<float> AsyncDistance(float[] embeddings1, float[] embeddings2, CancellationToken token)
        {
            var res = ExecuteAsync(embeddings1, embeddings2, Distance, token);
            return res;
        }

        public Task<float> AsyncSimilarity(float[] embeddings1, float[] embeddings2, CancellationToken token)
        {
            var res = ExecuteAsync(embeddings1, embeddings2, Similarity, token);
            return res;
        }

        public img_embed embedder;

        public img_comp()
        {
            this.embedder = new img_embed();
        }
        public  Task<float[]> EmbeddingsAsync(Image<Rgb24> face, CancellationToken token)
        {
            var res = embedder.GetEmbeddingsAsync(face, token);
            return res;
        }
    }
}