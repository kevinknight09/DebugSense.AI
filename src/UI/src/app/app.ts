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
  
  statusMessage: string = '';
  elapsedSeconds: number = 0;
  timerInterval: any;

  constructor(private cdr: ChangeDetectorRef) {}

  async sendQuery() {
    if (!this.query.trim()) return;

    this.isGenerating = true;
    this.responseContent = '';
    this.elapsedSeconds = 0;
    this.statusMessage = 'Vectorizing query & searching Qdrant...';
    
    // Start a timer to update the user on what is happening behind the scenes
    this.timerInterval = setInterval(() => {
      this.elapsedSeconds++;
      if (this.elapsedSeconds === 5) {
        this.statusMessage = 'Loading massive LLM into GPU (this can take up to 30s)...';
      }
      this.cdr.detectChanges();
    }, 1000);

    this.cdr.detectChanges(); // Force UI update

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

        // Clear the timer the moment we get the first token
        if (this.timerInterval) {
          clearInterval(this.timerInterval);
          this.timerInterval = null;
          this.statusMessage = 'Generating response...';
        }

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
      if (this.timerInterval) {
        clearInterval(this.timerInterval);
        this.timerInterval = null;
      }
      this.cdr.detectChanges();
    }
  }
}
