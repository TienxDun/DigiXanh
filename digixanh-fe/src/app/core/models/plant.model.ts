// DTO trả về từ danh sách (admin / public)
export interface PlantDto {
    id: number;
    name: string;
    scientificName: string;
    price: number;
    categoryName: string;
    imageUrl: string;
    createdAt: Date | string;
    stockQuantity?: number | null;
}

export interface PlantDetailDto {
    id: number;
    name: string;
    scientificName: string;
    description?: string | null;
    price: number;
    categoryId: number;
    categoryName: string;
    imageUrl: string;
    trefleId?: number | null;  // Giữ tên cũ để tương thích database, đây là external plant ID
    stockQuantity?: number | null;
}

// DTO cho danh mục (dropdown)
export interface CategoryDto {
    id: number;
    name: string;
}

// Kết quả tìm kiếm từ Perenual API
export interface PerenualSearchResult {
    id: number;
    commonName: string | null;
    scientificName: string;
    imageUrl: string | null;
    common_name?: string | null;
    scientific_name?: string;
    image_url?: string | null;
}

// Chi tiết cây từ Perenual API
export interface PerenualPlantDetail {
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

// Alias cho backward compatibility (sẽ bỏ sau khi refactor xong)
/** @deprecated Use PerenualSearchResult instead */
export type TrefleSearchResult = PerenualSearchResult;
/** @deprecated Use PerenualPlantDetail instead */
export type TreflePlantDetail = PerenualPlantDetail;

// Request tạo cây mới
export interface CreatePlantRequest {
    name: string;
    scientificName: string;
    description?: string;
    imageUrl?: string;
    price: number;
    categoryId?: number;
    trefleId?: number;  // Giữ tên cũ để tương thích database - đây là external plant ID
    stockQuantity?: number | null;
}
