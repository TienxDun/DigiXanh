import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { PlantDto } from '../../../core/models/plant.model';
import { resolvePlantImageUrl } from '../../../core/utils/image-url.util';

@Component({
    selector: 'app-plant-card',
    standalone: true,
    imports: [CommonModule, RouterLink],
    templateUrl: './plant-card.component.html',
    styleUrl: './plant-card.component.scss',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlantCardComponent {
    @Input({ required: true }) plant!: PlantDto;
    @Input() showBadge: boolean = true;

    readonly fallbackImageUrl = 'assets/images/plant-placeholder.svg';

    resolveImageUrl(imageUrl?: string | null): string {
        return resolvePlantImageUrl(imageUrl);
    }

    onImageError(event: Event): void {
        const target = event.target as HTMLImageElement | null;
        if (!target || target.src.endsWith(this.fallbackImageUrl)) {
            return;
        }
        target.src = this.fallbackImageUrl;
    }
}
