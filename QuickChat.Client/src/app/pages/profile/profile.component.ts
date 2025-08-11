import { CommonService } from './../../common.service';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ChatService } from '../../services/chat.service';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, FormsModule, CommonModule,RouterModule],
  templateUrl: './profile.component.html',
})
export class ProfileComponent implements OnInit {
  serverProfilePath?: string ='';
  serverCoverPath?: string = ';'
  profile: UserProfile = {}; // populate from API or service
  editMode = false;
  profileForm!: FormGroup;

  profileImagePreview: string | ArrayBuffer | null = null;
  coverImagePreview: string | ArrayBuffer | null = null;
  selectedProfileImageFile?: File | null = null;
  selectedCoverImageFile?: File | null = null;



  constructor(private fb: FormBuilder, private chatService: ChatService, private common: CommonService) { }

  ngOnInit() {
    this.loadUserProfile();
    this.initForm();
  }

  initForm() {
    this.profileForm = this.fb.group({
      email: [this.profile.email, [Validators.required, Validators.email]],
      fullName: [this.profile.fullName, Validators.required],
      parmanantAddress: [this.profile.parmanantAddress],
      presentAddress: [this.profile.presentAddress],
      phoneNumber: [this.profile.phoneNumber],
      universityName: [this.profile.universityName],
      collageName: [this.profile.collageName],
      workPlaceName: [this.profile.workPlaceName],
      coverImageUrl: [this.profile.coverImageUrl],
      profileImageUrl: [this.profile.profileImageUrl],

      dateOfBirth: [this.formatDateForInput(this.profile.dateOfBirth)],
    });
  }

  formatDateForInput(date?: string | Date) {
    if (!date) return null;
    const d = new Date(date);
    // yyyy-MM-dd format for input type date
    return d.toISOString().split('T')[0];
  }

  toggleEdit() {
    this.editMode = !this.editMode;
    if (this.editMode) {
      this.initForm(); // Reset form values on edit start
    }
  }

  async onSave() {
    if (this.profileForm.invalid) return;

    this.profile = { ...this.profile, ...this.profileForm.value };
    // 2. Assign profile image if selected
    if (this.selectedProfileImageFile) {
      this.profileImagePreview = null;
      this.selectedProfileImageFile = undefined;
      this.profile.profileImageUrl = this.serverProfilePath;
    }

    // 3. Assign cover image if selected
    if (this.selectedCoverImageFile) {
      this.coverImagePreview = null;
      this.selectedCoverImageFile = undefined;
      this.profile.coverImageUrl = this.serverCoverPath;
    }

    // Finally, send updated profile to backend to save
    this.chatService.updateUserProfile(this.profile)
      .subscribe({
        next: (res) => {
          alert(res.message);
          console.log('profile-update :', this.profile);
        },
        error: err => this.common.handleApiError(err)
      })

    this.toggleEdit();
  }

  async uploadFile(file: File): Promise<string | undefined> {
    if (!file) return;

    try {
      const uploadedUrl = await this.chatService.uploadFile(file);
      return uploadedUrl; // ✅ return the uploaded file URL
    } catch (err) {
      console.error('Error uploading file:', err);
      return undefined;
    }
  }

  // Image upload functionality.
  async onProfileImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedProfileImageFile = input.files[0];

      this.serverProfilePath = await this.uploadFile(input.files[0]);
      console.log('profile-server-url', this.serverProfilePath);
      const reader = new FileReader();
      reader.onload = e => this.profileImagePreview = reader.result;
      reader.readAsDataURL(this.selectedProfileImageFile);
    }
  }

  async onCoverImageSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.selectedCoverImageFile = input.files[0];

      this.serverCoverPath = await this.uploadFile(input.files[0]);
      const reader = new FileReader();
      reader.onload = e => this.coverImagePreview = reader.result;
      reader.readAsDataURL(this.selectedCoverImageFile);
    }
  }
  goToWelcome() {

  }
  loadUserProfile() {
    this.chatService.getCurrentUserProfile().subscribe((res) => {
      this.profile = {
        email: res.email,
        fullName: res.userName,
        profileImageUrl: res.profileImageUrl,
        coverImageUrl: res.coverImageUrl,
        parmanantAddress: res.parmanantAddress,
        presentAddress: res.presentAddress,
        phoneNumber: res.phoneNumber,
        universityName: res.universityName,
        collageName: res.collageName,
        workPlaceName: res.workPlaceName,
        dateOfBirth: res.dateOfBirth
      }
    });
    this.profileImagePreview = null;
    this.coverImagePreview = null;
  }

}

interface UserProfile {
  email?: string;
  fullName?: string;
  profileImageUrl?: string;
  coverImageUrl?: string;
  parmanantAddress?: string;
  presentAddress?: string;
  phoneNumber?: string;
  universityName?: string;
  collageName?: string;
  workPlaceName?: string;
  dateOfBirth?: string | Date;
}
