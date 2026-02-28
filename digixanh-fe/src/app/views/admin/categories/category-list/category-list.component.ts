import { Component, OnInit } from '@angular/core';
import { AsyncPipe, NgFor, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { AdminCategoryService, CreateCategoryRequest } from '../../../../core/services/admin-category.service';
import { CategoryDto } from '../../../../core/models/plant.model';

@Component({
  selector: 'app-category-list',
  standalone: true,
  imports: [NgIf, NgFor, AsyncPipe, FormsModule],
  templateUrl: './category-list.component.html',
  styleUrls: ['./category-list.component.scss']
})
export class CategoryListComponent implements OnInit {
  categories$ = new BehaviorSubject<CategoryDto[]>([]);
  loading$ = new BehaviorSubject<boolean>(false);
  actionMessage$ = new BehaviorSubject<string | null>(null);

  showModal = false;
  isEditMode = false;
  currentCategory: CreateCategoryRequest & { id?: number } = { name: '' };

  constructor(private adminCategoryService: AdminCategoryService) { }

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.loading$.next(true);
    this.adminCategoryService.getCategories().pipe(
      finalize(() => this.loading$.next(false))
    ).subscribe({
      next: (data) => {
        this.categories$.next(data || []);
      },
      error: () => {
        this.actionMessage$.next('Không thể tải danh sách danh mục.');
      }
    });
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.currentCategory = { name: '' };
    this.showModal = true;
  }

  openEditModal(category: CategoryDto): void {
    this.isEditMode = true;
    this.currentCategory = { id: category.id, name: category.name };
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.currentCategory = { name: '' };
  }

  saveCategory(): void {
    if (!this.currentCategory.name.trim()) {
      return;
    }

    this.loading$.next(true);
    if (this.isEditMode && this.currentCategory.id) {
      this.adminCategoryService.updateCategory(this.currentCategory.id, this.currentCategory).pipe(
        finalize(() => this.loading$.next(false))
      ).subscribe({
        next: () => {
          this.actionMessage$.next('Cập nhật danh mục thành công.');
          this.closeModal();
          this.loadCategories();
        },
        error: () => {
          this.actionMessage$.next('Cập nhật thất bại. Vui lòng thử lại.');
          this.closeModal();
        }
      });
    } else {
      this.adminCategoryService.createCategory(this.currentCategory).pipe(
        finalize(() => this.loading$.next(false))
      ).subscribe({
        next: () => {
          this.actionMessage$.next('Thêm danh mục thành công.');
          this.closeModal();
          this.loadCategories();
        },
        error: () => {
          this.actionMessage$.next('Thêm mới thất bại. Vui lòng thử lại.');
          this.closeModal();
        }
      });
    }
  }

  deleteCategory(id: number): void {
    if (confirm('Bạn có chắc chắn muốn xoá danh mục này?')) {
      this.loading$.next(true);
      this.adminCategoryService.deleteCategory(id).pipe(
        finalize(() => this.loading$.next(false))
      ).subscribe({
        next: () => {
          this.actionMessage$.next('Xoá danh mục thành công.');
          this.loadCategories();
        },
        error: () => {
          this.actionMessage$.next('Xoá thất bại. Vui lòng thử lại.');
        }
      });
    }
  }
}
