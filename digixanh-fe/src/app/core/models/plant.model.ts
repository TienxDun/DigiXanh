// DTO trả về từ danh sách (admin / public)
export interface PlantDto {
    id: number;
    name: string;
    scientificName: string;
    price: number;
    categoryName: string;
    imageUrl: string;
    createdAt: Date | string;
}

export interface PlantDetailDto {
    id: number;
    name: string;
    scientificName: string;
    description?: string | null;
    price: number;
    categoryId: number;
    imageUrl: string;
    trefleId?: number | null;
}

// DTO cho danh mục (dropdown)
export interface CategoryDto {
    id: number;
    name: string;
}

// Kết quả tìm kiếm từ Trefle proxy
export interface TrefleSearchResult {
    id: number;
    commonName: string | null;
    scientificName: string;
    imageUrl: string | null;
    common_name?: string | null;
    scientific_name?: string;
    image_url?: string | null;
}

// Chi tiết cây từ Trefle proxy
export interface TreflePlantDetail {
    id: number;
    commonName: string | null;
    scientificName: string;
    description?: string | null;
    imageUrl: string | null;
    family?: string | null;
    genus?: string | null;
    common_name?: string | null;
    scientific_name?: string;
    image_url?: string | null;
}

// Request tạo cây mới
export interface CreatePlantRequest {
    name: string;
    scientificName: string;
    description?: string;
    imageUrl?: string;
    price: number;
    categoryId?: number;
    trefleId?: number;
}

