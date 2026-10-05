import '@testing-library/jest-dom';

Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  }),
});

// Mock Image to prevent external network fetching and unhandled errors in JSDOM
class MockImage {
  onload: (() => void) | null = null;
  onerror: (() => void) | null = null;
  private _src = '';

  get src() {
    return this._src;
  }

  set src(value: string) {
    this._src = value;
    setTimeout(() => {
      this.onload?.();
    }, 0);
  }

  alt = '';
  width = 100;
  height = 100;

  addEventListener(event: string, handler: () => void) {
    if (event === 'load') this.onload = handler;
    if (event === 'error') this.onerror = handler;
  }

  removeEventListener() {}
}

(globalThis as unknown as Record<string, unknown>).Image = MockImage;
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).Image = MockImage;
}

// Mock XMLHttpRequest to prevent Leaflet tile and unhandled JSDOM AggregateError noise
class MockXMLHttpRequest {
  open = () => {};
  send = () => {};
  setRequestHeader = () => {};
  abort = () => {};
  addEventListener = () => {};
  removeEventListener = () => {};
  dispatchEvent = () => false;
  readyState = 4;
  status = 200;
  statusText = 'OK';
  responseText = '';
  response = '';
  onreadystatechange = null;
  onload = null;
  onerror = null;
}

(globalThis as unknown as Record<string, unknown>).XMLHttpRequest = MockXMLHttpRequest;
if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).XMLHttpRequest = MockXMLHttpRequest;
}

