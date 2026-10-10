import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/api/client";
import type { Product } from "@/types/product";

// API Response Structure

interface ApiResponse<T> {
  source: string;
  executionSpeed: string;
  count: number;
  data: T;
}

// Define the worker

const fetchProducts = async() : Promise<Product[]> => {

    const response = await apiClient.get<ApiResponse<Product[]>>('/Products');
    return response.data.data;

}

// Setup Custom hook & Cacheing engine

export const useProducts = () => {

    return useQuery({
        queryKey: ['products'],
        queryFn: fetchProducts
    });

}