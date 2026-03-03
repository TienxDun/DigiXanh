import { Component, DestroyRef, OnInit, OnDestroy, inject, ChangeDetectorRef, ViewChild, ElementRef, AfterViewInit, HostListener } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { FormsModule } from '@angular/forms';
import { ToastContainerComponent } from '../../shared/components/toast/toast.component';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, AsyncPipe, ToastContainerComponent, FormsModule],
  templateUrl: './public-layout.component.html',
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent implements OnInit, OnDestroy, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly cartService = inject(CartService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly router = inject(Router);
  public authService = inject(AuthService);
  readonly cartCount$ = this.cartService.cartCount$;
  public isCheckoutFlow = false;

  @ViewChild('topObserver') topObserver!: ElementRef;
  private observer: IntersectionObserver | null = null;
  isScrolled = false;
  isMobileMenuOpen = false;

  // Search state
  isSearchVisible = false;
  searchQuery = '';

  @HostListener('document:keydown.escape')
  onEscKeydown() {
    if (this.isSearchVisible) {
      this.closeSearch();
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    if (!this.isSearchVisible) return;

    const searchWrapper = document.querySelector('.search-wrapper');
    if (searchWrapper && !searchWrapper.contains(event.target as Node)) {
      this.closeSearch();
    }
  }

  toggleSearch() {
    if (this.isSearchVisible) {
      if (this.searchQuery.trim()) {
        this.onSearchSubmit();
      } else {
        this.closeSearch();
      }
    } else {
      this.openSearch();
    }
  }

  openSearch() {
    this.isSearchVisible = true;
    setTimeout(() => {
      const searchInput = document.getElementById('headerSearchInput');
      searchInput?.focus();
    }, 300);
  }

  closeSearch() {
    this.isSearchVisible = false;
    this.searchQuery = '';
  }

  onSearchSubmit() {
    if (this.searchQuery.trim()) {
      this.router.navigate(['/plants'], {
        queryParams: { search: this.searchQuery.trim() }
      });
      this.closeSearch();
    }
  }

  // Footer mobile states
  activeFooterSection: string | null = null;

  toggleFooterSection(section: string) {
    if (window.innerWidth >= 768) return;
    this.activeFooterSection = this.activeFooterSection === section ? null : section;
  }

  isFooterSectionOpen(section: string): boolean {
    if (window.innerWidth >= 768) return true;
    return this.activeFooterSection === section;
  }

  toggleMobileMenu() {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    // Ngăn scroll khi menu mở
    if (this.isMobileMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
  }

  closeMobileMenu() {
    this.isMobileMenuOpen = false;
    document.body.style.overflow = '';
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.observer = new IntersectionObserver(([entry]) => {
      const scrolled = !entry.isIntersecting;
      if (this.isScrolled !== scrolled) {
        this.isScrolled = scrolled;
        this.cdr.detectChanges();
      }
    }, { threshold: 0 });

    if (this.topObserver?.nativeElement) {
      this.observer.observe(this.topObserver.nativeElement);
    }
  }

  ngOnDestroy() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  constructor() {
    // Theo dõi route để ẩn/hiện Bottom Nav
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd),
      takeUntilDestroyed(this.destroyRef)
    ).subscribe(() => {
      const url = this.router.url;
      this.isCheckoutFlow = url.includes('/cart') || url.includes('/checkout');
      this.cdr.detectChanges();
    });

    this.authService.isAuthenticated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((isAuthenticated) => {
        if (!isAuthenticated) {
          this.cartService.resetCartCount();
          return;
        }

        this.cartService.refreshCartCount().subscribe();
      });
  }

  logout(): void {
    this.authService.logout();
  }
}