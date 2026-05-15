# Consuming the File API from Angular 21

A practical guide for the SPA. Covers upload, inline view, download, delete,
plus the modern Angular 21 patterns: standalone components, signals,
`HttpClient` with `withFetch()`, `httpResource`, deferred loading, and the
new control-flow syntax.

The backend reference is the file-handling refactor in
[file-handling-refactor.md](./file-handling-refactor.md). This doc only covers
the Angular side.

---

## Table of contents

1. [API surface — what the backend exposes](#1-api-surface)
2. [HTTP client setup](#2-http-client-setup)
3. [Auth + cookies](#3-auth--cookies)
4. [`FilesService` — typed wrapper around the API](#4-filesservice--typed-wrapper)
5. [Inline view — `<img>` and `<a>`](#5-inline-view)
6. [Upload component with progress + signals](#6-upload-component)
7. [Download with progress + filename](#7-download-with-progress--filename)
8. [Delete](#8-delete)
9. [Caching strategy and ETags](#9-caching-strategy-and-etags)
10. [Optional: `httpResource` for read-only file metadata](#10-httpresource-for-read-only-file-metadata)
11. [Common pitfalls](#11-common-pitfalls)

---

## 1. API surface

| Method | Path | Body | Returns | Auth |
|---|---|---|---|---|
| `GET` | `/api/files/{id}` | — | file bytes (inline) | anonymous |
| `GET` | `/api/files/{id}/download` | — | file bytes (attachment) | required |
| `DELETE` | `/api/files/{id}` | — | `204 No Content` | required |
| `POST` | (feature endpoints) | `multipart/form-data` | `{ FileId, Url }` etc. | required |

Inline GET responses include:

- `ETag: "<sha256>"` (or `W/"<id>-<size>"` for legacy rows)
- `Cache-Control: private, max-age=86400, must-revalidate`
- `Content-Disposition: inline; filename*=UTF-8''…`
- `Accept-Ranges: bytes`

Implication: just put the URL into `<img src>` and the browser does the
right thing. No `HttpClient` involvement is needed for *display*.

---

## 2. HTTP client setup

Angular 21 ships `provideHttpClient()` and `withFetch()` (Fetch API instead of
XHR — better streaming, cancellation, less code, no jQuery-era quirks).

`app.config.ts`:

```ts
import { ApplicationConfig, provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import { errorInterceptor } from './core/http/error.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZonelessChangeDetection(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, errorInterceptor]),
    ),
  ],
};
```

Why each piece:

- `provideZonelessChangeDetection()` — Angular 21's recommended setup. Works
  hand-in-hand with signals.
- `withFetch()` — uses the browser's native `fetch`. Required for
  `httpResource` and gives you proper streaming + `AbortController`.
- `withInterceptors([...])` — functional interceptors, the modern pattern.

---

## 3. Auth + cookies

The backend uses HttpOnly cookie auth + a `XSRF-TOKEN` cookie / `X-XSRF-TOKEN`
header double-submit pattern. Two non-negotiables:

### 3.1 Send cookies cross-origin

If the API and SPA are on different origins, every request must use
`withCredentials: true`. The cleanest way is an interceptor:

```ts
// core/http/credentials.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ withCredentials: true }));
```

### 3.2 CSRF header for unsafe methods

Read the `XSRF-TOKEN` cookie and mirror it into `X-XSRF-TOKEN` on
`POST/PUT/PATCH/DELETE`:

```ts
// core/http/csrf.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';

const UNSAFE = new Set(['POST', 'PUT', 'PATCH', 'DELETE']);

function readCookie(name: string): string | null {
  const m = document.cookie.match(new RegExp(`(^|;\\s*)${name}=([^;]+)`));
  return m ? decodeURIComponent(m[2]) : null;
}

export const csrfInterceptor: HttpInterceptorFn = (req, next) => {
  if (!UNSAFE.has(req.method)) return next(req);

  const token = readCookie('XSRF-TOKEN');
  if (!token) return next(req);

  return next(req.clone({ setHeaders: { 'X-XSRF-TOKEN': token } }));
};
```

Register both:

```ts
provideHttpClient(
  withFetch(),
  withInterceptors([credentialsInterceptor, csrfInterceptor, errorInterceptor]),
),
```

> Note: Angular ships a built-in `withXsrfConfiguration({ cookieName: 'XSRF-TOKEN', headerName: 'X-XSRF-TOKEN' })` but it only fires for *same-origin* `mutating` requests. The interceptor above is the safer, explicit version that works cross-origin too.

---

## 4. `FilesService` — typed wrapper

Standalone, `providedIn: 'root'`, signal-friendly. One service for every file
operation; everything else in the app uses it.

```ts
// core/files/files.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpRequest } from '@angular/common/http';
import { Observable, filter, map } from 'rxjs';

export interface UploadProgress {
  loaded: number;
  total: number | null;
  percent: number | null;
}

export type UploadEvent<T> =
  | { kind: 'progress'; progress: UploadProgress }
  | { kind: 'response'; body: T };

export interface UploadImageResponse { fileId: string; url: string; }
export interface UploadFileResponse  { fileId: string; }

@Injectable({ providedIn: 'root' })
export class FilesService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api';

  /** Stable URL for `<img src>` / `<a href>`. */
  url(fileId: string): string {
    return `${this.base}/files/${fileId}`;
  }

  /** Force-download URL. Browsers honour the attachment disposition. */
  downloadUrl(fileId: string): string {
    return `${this.base}/files/${fileId}/download`;
  }

  /**
   * Upload an image to a feature endpoint (e.g. profile avatar).
   * Emits progress events while bytes are flying, then a single response.
   */
  uploadImage(
    endpoint: string,
    entityType: string,
    entityId: string,
    folder: string,
    file: File,
  ): Observable<UploadEvent<UploadImageResponse>> {
    const form = new FormData();
    form.append('EntityType', entityType);
    form.append('EntityId', entityId);
    form.append('folder', folder);
    form.append('Image', file, file.name);

    const req = new HttpRequest('POST', `${this.base}${endpoint}`, form, {
      reportProgress: true,
    });

    return this.http.request<UploadImageResponse>(req).pipe(
      map(toUploadEvent<UploadImageResponse>),
      filter((e): e is UploadEvent<UploadImageResponse> => e !== null),
    );
  }

  delete(fileId: string) {
    return this.http.delete<void>(`${this.base}/files/${fileId}`);
  }

  /**
   * Download the bytes as a Blob, with progress. Kick the resulting Blob
   * into a hidden anchor or `URL.createObjectURL` to trigger save.
   */
  downloadBlob(fileId: string): Observable<UploadEvent<Blob>> {
    const req = new HttpRequest('GET', `${this.base}/files/${fileId}/download`, null, {
      reportProgress: true,
      responseType: 'blob',
    });

    return this.http.request<Blob>(req).pipe(
      map(toDownloadEvent),
      filter((e): e is UploadEvent<Blob> => e !== null),
    );
  }
}

function toUploadEvent<T>(ev: HttpEvent<T>): UploadEvent<T> | null {
  switch (ev.type) {
    case HttpEventType.UploadProgress: {
      const total = ev.total ?? null;
      const percent = total ? Math.round((ev.loaded / total) * 100) : null;
      return { kind: 'progress', progress: { loaded: ev.loaded, total, percent } };
    }
    case HttpEventType.Response:
      return { kind: 'response', body: ev.body as T };
    default:
      return null;
  }
}

function toDownloadEvent(ev: HttpEvent<Blob>): UploadEvent<Blob> | null {
  switch (ev.type) {
    case HttpEventType.DownloadProgress: {
      const total = ev.total ?? null;
      const percent = total ? Math.round((ev.loaded / total) * 100) : null;
      return { kind: 'progress', progress: { loaded: ev.loaded, total, percent } };
    }
    case HttpEventType.Response:
      return { kind: 'response', body: ev.body as Blob };
    default:
      return null;
  }
}
```

Why this shape:

- `url()` / `downloadUrl()` return *paths*, not observables. Display does not need RxJS.
- `uploadImage` accepts the feature endpoint (e.g. `/profile/avatar`) so a single helper covers every upload site without hard-coding routes.
- `UploadEvent<T>` is a discriminated union — TypeScript narrows on `kind`.
- Progress + response are emitted as one stream so consumers can drive a progress bar and a "done" handler off the same subscription.

---

## 5. Inline view

The simplest path. Browser does all the work, the API URL is stable, the
backend already emits `Cache-Control` so the browser caches it.

```html
<!-- profile-avatar.component.html -->
@if (avatarFileId()) {
  <img [src]="files.url(avatarFileId()!)"
       [alt]="userName()"
       width="64" height="64"
       loading="lazy"
       decoding="async" />
} @else {
  <div class="avatar-placeholder">{{ initials() }}</div>
}
```

```ts
// profile-avatar.component.ts
import { Component, computed, inject, input } from '@angular/core';
import { FilesService } from '../core/files/files.service';

@Component({
  selector: 'app-profile-avatar',
  templateUrl: './profile-avatar.component.html',
})
export class ProfileAvatarComponent {
  readonly files = inject(FilesService);
  readonly userName = input.required<string>();
  readonly avatarFileId = input<string | null>(null);
  readonly initials = computed(() => this.userName().split(' ').map(s => s[0]).join('').slice(0, 2).toUpperCase());
}
```

Notes:

- `loading="lazy"` defers the request until the `<img>` is near the viewport.
- `decoding="async"` lets the browser decode off the main thread.
- No `HttpClient` is involved. The browser sees the URL, requests it, applies the `ETag` and `Cache-Control` from the server, and reuses the cached bytes on the next page nav.

---

## 6. Upload component

A compact upload card with drag-and-drop, progress bar, error state, signals,
and the new control flow. This is the pattern to copy for every upload UI.

```ts
// features/profile/avatar-uploader.component.ts
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FilesService, UploadProgress } from '../../core/files/files.service';

type UploadState =
  | { kind: 'idle' }
  | { kind: 'uploading'; progress: UploadProgress }
  | { kind: 'success'; fileId: string; url: string }
  | { kind: 'error'; message: string };

@Component({
  selector: 'app-avatar-uploader',
  templateUrl: './avatar-uploader.component.html',
  styleUrl: './avatar-uploader.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AvatarUploaderComponent {
  private readonly files = inject(FilesService);
  private readonly destroyRef = inject(DestroyRef);

  readonly state = signal<UploadState>({ kind: 'idle' });
  readonly isBusy = computed(() => this.state().kind === 'uploading');

  /** Triggered by both the file input and a drop. */
  onFile(file: File | null | undefined): void {
    if (!file) return;

    if (!file.type.startsWith('image/')) {
      this.state.set({ kind: 'error', message: 'Only image files are allowed.' });
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.state.set({ kind: 'error', message: 'Max 10 MB.' });
      return;
    }

    this.state.set({
      kind: 'uploading',
      progress: { loaded: 0, total: file.size, percent: 0 },
    });

    this.files
      .uploadImage('/profile/avatar', 'User', /* userId */ '', 'Profiles', file)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ev) => {
          if (ev.kind === 'progress') {
            this.state.set({ kind: 'uploading', progress: ev.progress });
          } else {
            this.state.set({ kind: 'success', fileId: ev.body.fileId, url: ev.body.url });
          }
        },
        error: (err) => {
          this.state.set({ kind: 'error', message: extractMessage(err) });
        },
      });
  }

  /** Drop-handler entry point. Used by `(drop)` in the template. */
  onDrop(ev: DragEvent): void {
    ev.preventDefault();
    this.onFile(ev.dataTransfer?.files?.[0]);
  }

  reset(): void {
    this.state.set({ kind: 'idle' });
  }
}

function extractMessage(err: unknown): string {
  if (err && typeof err === 'object' && 'error' in err) {
    const body = (err as { error: unknown }).error as { detail?: string; title?: string } | string;
    if (typeof body === 'string') return body;
    return body?.detail ?? body?.title ?? 'Upload failed.';
  }
  return 'Upload failed.';
}
```

Template using the new `@if` / `@switch` control flow:

```html
<!-- avatar-uploader.component.html -->
<div class="uploader"
     (dragover)="$event.preventDefault()"
     (drop)="onDrop($event)">

  @switch (state().kind) {
    @case ('idle') {
      <label class="drop-zone">
        <input type="file"
               accept="image/*"
               hidden
               (change)="onFile($any($event.target).files?.[0])" />
        <span>Drop image or click to browse</span>
      </label>
    }

    @case ('uploading') {
      @let p = state().progress;
      <div class="progress">
        <progress [value]="p.percent ?? 0" max="100"></progress>
        <small>{{ p.percent ?? '?' }}% — {{ p.loaded | number }} / {{ p.total | number }} bytes</small>
      </div>
    }

    @case ('success') {
      <div class="ok">
        <img [src]="state().url" alt="" width="64" height="64" />
        <button type="button" (click)="reset()">Replace</button>
      </div>
    }

    @case ('error') {
      <div class="err" role="alert">
        {{ state().message }}
        <button type="button" (click)="reset()">Try again</button>
      </div>
    }
  }
</div>
```

What's idiomatic in Angular 21:

- `signal<UploadState>(...)` for component state, no `BehaviorSubject`.
- `computed(...)` for derived values like `isBusy`.
- `ChangeDetectionStrategy.OnPush` is implicit for signal components but still good to declare.
- `takeUntilDestroyed(destroyRef)` to scope subscriptions to component lifetime.
- `@if` / `@switch` / `@case` / `@let` instead of structural directives.
- `input.required<T>()` / `input<T>()` instead of `@Input()` decorators (used in the avatar component above).

---

## 7. Download with progress + filename

Browser-native saves are best. For an "attachment" link, just use the download
URL — the backend's `Content-Disposition: attachment` header triggers Save:

```html
<a [href]="files.downloadUrl(fileId)">Download</a>
```

When you need a progress bar (large reports / exports), use `downloadBlob`:

```ts
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FilesService, UploadProgress } from '../../core/files/files.service';

@Component({
  selector: 'app-download-button',
  template: `
    <button type="button" (click)="start()" [disabled]="busy()">
      @if (busy()) {
        Downloading… {{ progress()?.percent ?? '?' }}%
      } @else {
        Download
      }
    </button>
  `,
})
export class DownloadButtonComponent {
  private readonly files = inject(FilesService);
  private readonly destroyRef = inject(DestroyRef);

  fileId = '';
  fileName = 'file.bin';

  readonly busy = signal(false);
  readonly progress = signal<UploadProgress | null>(null);

  start(): void {
    if (this.busy() || !this.fileId) return;
    this.busy.set(true);

    this.files
      .downloadBlob(this.fileId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ev) => {
          if (ev.kind === 'progress') this.progress.set(ev.progress);
          else this.saveBlob(ev.body, this.fileName);
        },
        complete: () => this.busy.set(false),
        error: () => this.busy.set(false),
      });
  }

  private saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  }
}
```

The trick is `URL.createObjectURL(blob)` + an off-DOM anchor. `URL.revokeObjectURL` releases the in-memory blob immediately after the click — important for big files because the blob lives on the heap until you do.

---

## 8. Delete

Bog-standard `DELETE`. Nothing fancy needed, just the CSRF interceptor doing
its job.

```ts
@Component({
  /* ... */
  template: `
    <button type="button" (click)="onDelete()" [disabled]="busy()">
      @if (busy()) { Removing… } @else { Remove }
    </button>
  `,
})
export class FileRowComponent {
  private readonly files = inject(FilesService);
  private readonly destroyRef = inject(DestroyRef);

  fileId = input.required<string>();
  removed = output<string>();

  readonly busy = signal(false);

  onDelete(): void {
    this.busy.set(true);
    this.files.delete(this.fileId())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.removed.emit(this.fileId()),
        complete: () => this.busy.set(false),
        error: () => this.busy.set(false),
      });
  }
}
```

`output<T>()` is the Angular 21 replacement for `@Output() ... = new EventEmitter()`.

---

## 9. Caching strategy and ETags

The backend already does the heavy lifting:

- Strong `ETag` from SHA-256.
- `Cache-Control: private, max-age=86400, must-revalidate`.
- `304 Not Modified` short-circuits the body.

For the `<img src>` path, you literally do nothing. The browser caches the
image for a day, then revalidates with `If-None-Match` on day-2.

For the `HttpClient` paths, fetch's built-in cache also honours these headers
when the request mode is the default `default`. You can be explicit:

```ts
this.http.get(this.files.url(id), {
  responseType: 'blob',
  // pass through to fetch — Angular maps these.
  context: new HttpContext().set(USE_CACHE, true),
});
```

The cleaner pattern: don't fetch via `HttpClient` if you only need to display.
Use `<img>` / `<a>` and let the browser do its job.

### Manually busting the cache

When you replace an avatar, the file *id* changes — the URL is different — so
the browser does not need a cache-buster. If the id stays the same and the
content changed (a re-uploaded blob with the same id, which doesn't happen in
this codebase), append the hash as a query string:

```html
<img [src]="files.url(id) + '?v=' + hash" />
```

---

## 10. `httpResource` for read-only file metadata

Angular 21 ships `httpResource()` — a signal-driven resource that re-runs when
its inputs change, exposes `value()`, `isLoading()`, `error()`, and is
preferred over `effect()` + `HttpClient` for *read-only* fetching.

If you build a "files attached to entity X" view:

```ts
import { Component, inject, input } from '@angular/core';
import { httpResource } from '@angular/common/http';

interface FileSummary { id: string; fileName: string; sizeInBytes: number; contentType: string; }

@Component({
  selector: 'app-attachments',
  template: `
    @if (files.isLoading()) {
      <p>Loading…</p>
    } @else if (files.error()) {
      <p role="alert">Failed to load attachments.</p>
    } @else {
      <ul>
        @for (f of files.value() ?? []; track f.id) {
          <li>
            <a [href]="'/api/files/' + f.id">{{ f.fileName }}</a>
            ({{ f.sizeInBytes | number }} bytes)
          </li>
        }
      </ul>
    }
  `,
})
export class AttachmentsComponent {
  readonly entityId = input.required<string>();

  readonly files = httpResource<FileSummary[]>(() =>
    `/api/entities/${this.entityId()}/files`,
  );
}
```

When `entityId()` changes, the resource re-fetches. No subscriptions, no
`takeUntilDestroyed`, no manual loading flags.

> Note: `httpResource` requires `withFetch()` (which we registered in §2).

---

## 11. Common pitfalls

### 11.1 Using `HttpClient` to fetch image bytes for `<img>`

Don't. The browser caches `<img>` requests properly out of the box. If you
fetch via `HttpClient` and assign a blob URL, you opt out of the browser's
image cache and double the memory footprint.

```ts
// BAD — defeats the browser image cache
this.http.get(url, { responseType: 'blob' })
  .subscribe(blob => this.imageUrl.set(URL.createObjectURL(blob)));

// GOOD — let the browser handle it
<img [src]="files.url(fileId)" />
```

### 11.2 Forgetting `withCredentials` on cross-origin

If your SPA is on `app.example.com` and the API on `api.example.com`, cookies
will not be sent without `withCredentials: true`. The CSRF interceptor in §3
relies on the cookie reaching the server.

### 11.3 Reading the CSRF cookie before login

`XSRF-TOKEN` is set on the *response* of the login call. Until the user logs
in there's no cookie to mirror. The CSRF interceptor must skip silently when
the cookie is absent (which the example in §3 does).

### 11.4 Putting `accept` on the file input *and* validating client-side

`accept="image/*"` is a hint, not a guarantee — users can still pick anything.
Always re-validate type and size in `onFile`. The backend does the
authoritative check, but the client validation gives a fast UX.

### 11.5 Not revoking object URLs

```ts
const url = URL.createObjectURL(blob);
// ...
URL.revokeObjectURL(url); // the blob lives on the heap until this call
```

For a 50 MB download that's 50 MB of memory you don't recover.

### 11.6 Using `[src]` with template literals

```html
<!-- BAD — Angular re-evaluates the expression on every change detection,
     producing a new string identity each time even though the URL is stable -->
<img [src]="'/api/files/' + fileId() + '?cache-buster=' + Date.now()" />
```

Either bind to a stable URL (preferred) or compute it once with a
`computed()`.

### 11.7 `multipart/form-data` content type

Don't manually set `Content-Type: multipart/form-data` on the request. The
browser must add the boundary parameter; setting it explicitly strips the
boundary and the server can't parse the body. `FormData` plus *no*
content-type header is correct.

```ts
// BAD
this.http.post(url, form, { headers: { 'Content-Type': 'multipart/form-data' } });

// GOOD
this.http.post(url, form);
```

---

## TL;DR component checklist

Building a new file-related component? Walk down this list:

1. **Display only?** Use `<img [src]="files.url(id)" loading="lazy">` — done.
2. **Force download?** Use `<a [href]="files.downloadUrl(id)">` — done.
3. **Need progress?** Inject `FilesService`, subscribe to `uploadImage` /
   `downloadBlob`, drive a `signal<UploadState>`.
4. **Need a list?** `httpResource()` over your collection endpoint.
5. **Mutating?** Make sure `csrfInterceptor` and `credentialsInterceptor` are
   registered.
6. **Cleanup?** Always `takeUntilDestroyed(destroyRef)` and
   `URL.revokeObjectURL` in the blob path.
