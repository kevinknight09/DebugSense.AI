import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './app.html',
  styleUrls: ['./app.css']
})
export class App {
  query: string = '';
  responseContent: string = '';
  isGenerating: boolean = false;

  constructor(private cdr: ChangeDetectorRef) {}

  async sendQuery() {
    if (!this.query.trim()) return;

    this.isGenerating = true;
    this.responseContent = '';
    this.cdr.detectChanges(); // Force UI update to show 'Thinking...'

    try {
      const response = await fetch('http://localhost:5170/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ query: this.query })
      });

      if (!response.body) throw new Error('ReadableStream not supported.');

      const reader = response.body.getReader();
      const decoder = new TextDecoder('utf-8');

      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        const lines = chunk.split('\n\n');

        for (const line of lines) {
          if (line.startsWith('data: ')) {
            const text = line.replace('data: ', '').replace(/\\n/g, '\n');
            this.responseContent += text;
            this.cdr.detectChanges(); // Force Angular to paint the new token!
          }
        }
      }
    } catch (error) {
      console.error('Error fetching RAG response:', error);
      this.responseContent = 'An error occurred while connecting to the server.';
      this.cdr.detectChanges();
    } finally {
      this.isGenerating = false;
      this.cdr.detectChanges();
    }
  }
}
