import { HttpErrorResponse } from '@angular/common/http';
import { ApiResponse } from '../models/api.models';

export function errorMessage(err: unknown): string {
  if (err instanceof HttpErrorResponse) {
    if (err.status === 0) return 'Cannot reach the server. Is the API running?';

    const body = err.error as ApiResponse<Record<string, string[]>> | null;

    // validation failures put the field errors in data
    if (body?.data && typeof body.data === 'object') {
      const messages = Object.values(body.data).flat();
      if (messages.length) return messages.join(' ');
    }
    if (body?.message) return body.message;
  }
  return 'Something went wrong. Please try again.';
}